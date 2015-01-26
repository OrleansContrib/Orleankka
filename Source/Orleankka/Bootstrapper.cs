using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Providers;

namespace Orleankka
{
    public abstract class Bootstrapper : IBootstrapProvider
    {
        public string Name {get; private set;}

        Task IOrleansProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            return Init(config.Properties);
        }

        public abstract Task Init(IDictionary<string, string> properties);
    }
}