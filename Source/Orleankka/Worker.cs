using System;
using System.Linq;

namespace Orleankka
{
    public class Worker
    {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public class ConfigurationAttribute : ActorConfigurationAttribute
        {
            public ConfigurationAttribute(
                Concurrency concurrency = Concurrency.Sequential,
                Delivery delivery = Delivery.Ordered)
            {
                Configuration = ActorConfiguration.Worker(concurrency, delivery);
            }
        }
    }
}
