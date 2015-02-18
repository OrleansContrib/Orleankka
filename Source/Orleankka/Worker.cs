using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WorkerAttribute : ActorConfigurationAttribute
    {
        public WorkerAttribute()
        {
            Configuration = ActorConfiguration.Worker(Delivery.Ordered);
        }
    }
}
