using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Orleankka.Core
{
    interface IActorHost
    {
        IServiceProvider ServiceProvider { get; }
        IGrainIdentity Identity { get; }
        string IdentityString { get; }
        IGrainFactory GrainFactory { get; }
        Logger Logger();

        Task<object> Receive(object message);
        void DeactivateOnIdle();
        void DelayDeactivation(TimeSpan timeSpan);
        Task<IGrainReminder> GetReminder(string reminderName);
        Task<List<IGrainReminder>> GetReminders();
        Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);
        Task UnregisterReminder(IGrainReminder reminder);
        IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);
    }
}