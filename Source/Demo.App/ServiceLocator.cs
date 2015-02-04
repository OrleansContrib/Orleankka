using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleankka.Cluster;

namespace Demo
{
    public static class ServiceLocator
    {
        public static ITopicStorage TopicStorage
        {
            get; internal set;
        }

        public class Bootstrap : Bootstrapper
        {
            public override async Task Run(IDictionary<string, string> properties)
            {
                TopicStorage = await Demo.TopicStorage.Init(properties["account"]);
            }
        }
    }
}