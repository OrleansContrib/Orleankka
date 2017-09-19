﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Cluster;

namespace Demo
{
    public static class ServiceLocator
    {
        public static ITopicStorage TopicStorage
        {
            get; private set;
        }

        public class Bootstrap : Bootstrapper<IDictionary<string, string>>
        {
            static bool done;

            protected override async Task Run(IActorSystem system, IDictionary<string, string> properties)
            {
                if (done) 
                    throw new InvalidOperationException("Already done");

                TopicStorage = await Demo.TopicStorage.Init(properties["account"]);

                done = true;
            }
        }
    }
}