using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleankka;

namespace Demo
{
    public static class ServiceLocator
    {
        public static ITopicStorage TopicStorage
        {
            get; internal set;
        }
    }

    public class Bootstrap : ActorSystemBootstrapper
    {
        public override Task Run(IDictionary<string, string> properties)
        {
            ServiceLocator.TopicStorage = TopicStorage.Init(properties["account"]);
            return TaskDone.Done;
        }   
    }
}