using System;
using System.Threading.Tasks;

using Orleankka;
using Microsoft.WindowsAzure.Storage;

namespace Demo
{
    class Options
    {
        public CloudStorageAccount Account;
    }

    class DI : IActorActivator
    {
        ITopicStorage storage;

        public async Task Init(Options options)
        {
            storage = await TopicStorage.Init(options.Account);
        }

        public Actor Activate(Type type, string id, IActorRuntime runtime, Dispatcher dispatcher)
        {
            if (type == typeof(Api))
                return CreateApi(id, runtime);

            if (type == typeof(Topic))
                return CreateTopic(id, runtime);

            throw new InvalidOperationException($"Unknown actor type: {type}");
        }

        static Api CreateApi(string id, IActorRuntime runtime) => 
            new Api(id, runtime, new ObserverCollection(), ApiWorkerFactory.Create(id));

        Topic CreateTopic(string id, IActorRuntime runtime) => 
            new Topic(id, runtime, storage);
    }
}