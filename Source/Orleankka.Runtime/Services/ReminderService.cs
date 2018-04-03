using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Runtime;
using Orleans.Timers;

namespace Orleankka.Services
{
    using Utility;

    /// <summary>
    /// Manages registration of durable actor reminders
    /// </summary>
    public interface IReminderService
    {
        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (reminders) to the actor.
        /// If the current actor is deactivated when the reminder fires, a new activation of the actor will be created to receive this reminder.
        /// If an existing reminder with the same id already exists, than reminder will be overwritten with this new reminder.
        /// Reminders will always be received by one activation of this actor, even if multiple activations exist for this actor.
        /// 
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <param name="due">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for reminder registration.
        /// </returns>
        Task Register(string id, TimeSpan due, TimeSpan period);

        /// <summary>
        /// Unregister previously registered peristent reminder if any
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        Task Unregister(string id);

        /// <summary>
        /// Checks whether reminder with the given id is currently registered
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <returns><c>true</c> if reminder with the give name is currently registered, <c>false</c> otherwise </returns>
        Task<bool> IsRegistered(string id);

        /// <summary>
        /// Returns ids of all currently registered reminders
        /// </summary>
        /// <returns>Sequence of <see cref="string"/> elements</returns>
        Task<IEnumerable<string>> Registered();
    }

    /// <summary>
    /// Default runtime-bound implementation of <see cref="IReminderService"/>
    /// </summary>
    class ReminderService : IReminderService
    {
        readonly IDictionary<string, IGrainReminder> reminders = new Dictionary<string, IGrainReminder>();
        readonly IReminderRegistry registry;

        internal ReminderService(Grain grain)
        {
            registry = grain.Runtime().ReminderRegistry;
        }

        async Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            reminders[id] = await registry.RegisterOrUpdateReminder(id, due, period);
        }

        async Task IReminderService.Unregister(string id)
        {
            var reminder = reminders.Find(id) ?? await registry.GetReminder(id);
            
            if (reminder != null)
                await TryUnregisterReminder(reminder);

            reminders.Remove(id);
        }

        async Task TryUnregisterReminder(IGrainReminder reminder)
        {
            try
            {
                await registry.UnregisterReminder(reminder);
            }
            catch (ReminderException)
            {
                var fresh = await registry.GetReminder(reminder.ReminderName);
                if (fresh == null)
                    return;

                throw;
            }
        }

        async Task<bool> IReminderService.IsRegistered(string id)
        {
            var registered = await registry.GetReminder(id) != null;

            if (!registered && reminders.ContainsKey(id))
                reminders.Remove(id);

            return registered;
        }

        async Task<IEnumerable<string>> IReminderService.Registered()
        {
            return (await registry.GetReminders()).Select(x => x.ReminderName);
        }
    }
}