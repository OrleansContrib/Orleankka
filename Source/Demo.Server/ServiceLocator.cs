using System.Collections.Generic;
using System.Threading.Tasks;

using Orleans;
using Orleankka;

namespace Demo
{
    public class ServiceLocator : Bootstrapper
    {
        public static ITopicStorage TopicStorage { get; private set; }
    
        public override Task Run(IDictionary<string, string> properties)
        {
            TopicStorage = Demo.TopicStorage.Init(properties["account"]);
            return TaskDone.Done;
        }
    }
}