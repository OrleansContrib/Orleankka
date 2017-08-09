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
                return new Api(new ObserverCollection(), ApiWorkerFactory.Create(id));

            if (type == typeof(Topic))
                return new Topic(storage);

            throw new InvalidOperationException($"Unknown actor type: {type}");
        }
    }
}