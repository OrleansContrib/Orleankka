using System;
using System.Linq;

using Orleankka;
using Orleankka.Behaviors;

namespace Example
{
    class Supervision
    {
        readonly ActorGrain actor;
        readonly TimeSpan pingback;
        readonly string reminderName;

        public Supervision(ActorGrain actor, TimeSpan? pingback = null)
        {
            this.actor = actor;
            this.pingback = pingback ?? TimeSpan.FromMinutes(1);
            reminderName = $"{actor.GetType().Name}_keepalive";
        }

        public Receive On(Receive next)
        {
            return async message =>
            {
                switch (message)
                {
                    case Become _:
                        await actor.Reminders.Register(reminderName, due: pingback, period: pingback);
                        break;
                    case Reminder reminder when reminder.Name == reminderName:
                        return Done.Result;
                }

                return next(message);
            };
        }

        public Receive Off(Receive next)
        {
            return async message =>
            {
                var result = await next(message);

                switch (message)
                {
                    case Become _:
                        await actor.Reminders.Unregister(reminderName);
                        break;
                    case Reminder reminder when reminder.Name == reminderName:
                        await actor.Reminders.Unregister(reminderName);
                        return TaskResult.Done;
                }

                return result;
            };
        }
    }
}