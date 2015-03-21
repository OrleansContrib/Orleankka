using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka.Cluster;

namespace Demo
{
    public static class ServiceLocator
    {
        public static ITopicStorage TopicStorage
        {
            get; private set;
        }

        public class Bootstrap : Bootstrapper
        {
            static bool done;

            public override async Task Run(IDictionary<string, string> properties)
            {
                if (done) 
                    throw new InvalidOperationException("Already done");

                TopicStorage = await Demo.TopicStorage.Init(properties["account"]);

                done = true;
            }
        }
    }
}