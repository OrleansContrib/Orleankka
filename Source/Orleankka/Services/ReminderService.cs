using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka.Core;

using Orleans.Runtime;

namespace Orleankka.Services
{
    /// <summary>
    /// Manages registration of durable actor reminders
    /// </summary>
    public interface IReminderService
    {
        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (reminders) to the actor.
        /// The actor must implement the <see cref="Actor.OnReminder"/> method.
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
    public class ReminderService : IReminderService
    {
        readonly IDictionary<string, IGrainReminder> reminders = new Dictionary<string, IGrainReminder>();
        readonly Func<IActorEndpointReminderService> service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderService"/> class.
        /// </summary>
        /// <param name="actor">The dynamic actor which requires reminder services.</param>
        public ReminderService(Actor actor)
            : this(() => actor.Endpoint)
        {}

        ReminderService(Func<IActorEndpointReminderService> service)
        {
            this.service = service;
        }

        async Task IReminderService.Register(string id, TimeSpan due, TimeSpan period)
        {
            reminders[id] = await service().RegisterOrUpdateReminder(id, due, period);
        }

        async Task IReminderService.Unregister(string id)
        {
            var reminder = reminders.Find(id) ?? await service().GetReminder(id);
            
            if (reminder != null)
                await service().UnregisterReminder(reminder);

            reminders.Remove(id);
        }

        async Task<bool> IReminderService.IsRegistered(string id)
        {
            return reminders.ContainsKey(id) || (await service().GetReminder(id)) != null;
        }

        async Task<IEnumerable<string>> IReminderService.Registered()
        {
            return (await service().GetReminders()).Select(x => x.ReminderName);
        }
    }
}