using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Runtime;

namespace Orleankka.Core
{
    interface IGrainActivationService
    {
        void DeactivateOnIdle();
        void DelayDeactivation(TimeSpan timeSpan);
    }

    interface IGrainReminderService
    {
        Task<IGrainReminder> GetReminder(string reminderName);
        Task<List<IGrainReminder>> GetReminders();
        Task<IGrainReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);
        Task UnregisterReminder(IGrainReminder reminder);
    }

    interface IGrainTimerService
    {
        IDisposable RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);
    }
}
