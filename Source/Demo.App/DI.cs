using System;
using System.Threading.Tasks;

using Orleankka;
using Microsoft.WindowsAzure.Storage;

using Orleans.Runtime;

namespace Demo
{
    class Options
    {
        public CloudStorageAccount Account;
    }

    class DI : IGrainActivator
    {
        ITopicStorage storage;

        public async Task Init(Options options)
        {
            storage = await TopicStorage.Init(options.Account);
        }

        public object Create(IGrainActivationContext context)
        {
            var type = context.GrainType;
            var id = context.GrainIdentity.PrimaryKeyString;

            if (type == typeof(Api))
                return new Api(new ObserverCollection(), ApiWorkerFactory.Create(id));

            if (type == typeof(Topic))
                return new Topic(storage);

            throw new InvalidOperationException($"Unknown actor type: {type}");
        }

        public void Release(IGrainActivationContext context, object grain) {}
    }
}