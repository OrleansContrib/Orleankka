using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WorkerAttribute : ActorConfigurationAttribute
    {
        public WorkerAttribute(Delivery delivery = Delivery.Ordered)
        {
            Configuration = ActorConfiguration.Worker(delivery);
        }
    }
}
