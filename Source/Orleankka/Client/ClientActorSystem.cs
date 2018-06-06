using System;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using System.Collections.Generic;
    using System.Linq;

    using Core;
    using Utility;

    /// <summary>
    /// Client-side actor system interface
    /// </summary>
    public interface IClientActorSystem : IActorSystem
    {
        /// <summary>
        /// Creates new <see cref="IClientObservable"/>
        /// </summary>
        /// <returns>New instance of <see cref="IClientObservable"/></returns>
        Task<IClientObservable> CreateObservable();
    }

    /// <summary>
    /// Client-side actor system
    /// </summary>
    public sealed class ClientActorSystem : ActorSystem, IClientActorSystem, IDisposable
    {
        readonly ClientConfiguration configuration;
        readonly Action<IClientBuilder> builder;
        readonly Assembly[] assemblies;
        readonly Action<IServiceCollection> di;

        internal ClientActorSystem(
            ClientConfiguration configuration,
            Action<IClientBuilder> builder,
            Assembly[] assemblies,
            Action<IServiceCollection> di,
            IActorRefInvoker invoker) 
            : base(invoker)
        {
            this.configuration = configuration;
            this.builder = builder;
            this.assemblies = assemblies;
            this.di = di;
        }

        /// <inheritdoc />
        public async Task<IClientObservable> CreateObservable()
        {
            var proxy = await ClientEndpoint.Create(GrainFactory);
            return new ClientObservable(proxy);
        }

        /// <summary>
        /// Returns underlying <see cref="IClusterClient"/> instance
        /// </summary>
        public IClusterClient Client { get; private set; }

        /// <summary>
        /// Checks whether this client has been successfully connected (ie initialized)
        /// </summary>
        public bool Connected => Client?.IsInitialized ?? false;

        /// <summary>
        /// Connects this instance of client actor system to cluster
        /// </summary>
        /// <param name="retries">Number of retries in case on failure</param>
        /// <param name="retryTimeout">Timeout between retries. Default is 5 seconds</param>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="retries"/> argument value is less than 0</exception>
        public async Task Connect(int retries = 0, TimeSpan? retryTimeout = null)
        {
            if (retryTimeout == null)
                retryTimeout = TimeSpan.FromSeconds(5);

            if (retries < 0)
                throw new ArgumentOutOfRangeException(nameof(retries), 
                    "retries should be greater than or equal to 0");

            while (!Connected && retries-- >= 0)
            {
                try
                {
                    Client = Build();

                    using (Trace.Execution("Orleans client connection"))
                        await Client.Connect().ConfigureAwait(false);

                    Initialize(Client.ServiceProvider);
                }
                catch (Exception ex)
                {
                    if (retries >= 0)
                    {
                        System.Diagnostics.Trace.TraceWarning($"Can't connect to cluster. Trying again in {(int)retryTimeout.Value.TotalSeconds} seconds ... Got error: /n{ex}");
                        await Task.Delay(retryTimeout.Value).ConfigureAwait(false);
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceError($"Can't connect to cluster. Max retries reached. Got error: /n{ex}");
                        throw;
                    }
                }
            }
        }

        IClusterClient Build()
        {
            using (Trace.Execution("Orleans client initialization"))
            {
                var cb = new ClientBuilder();
                cb.UseConfiguration(configuration);
                builder?.Invoke(cb);

                cb.ConfigureServices(services =>
                {
                    services.Add(ServiceDescriptor.Singleton<IActorSystem>(this));
                    services.Add(ServiceDescriptor.Singleton<IClientActorSystem>(this));
                    services.Add(ServiceDescriptor.Singleton(this));

                    di?.Invoke(services);
                });

                var parts = new List<Assembly>(assemblies) { Assembly.GetExecutingAssembly() };
                parts.AddRange(ActorInterface.Registered().Select(x => x.Grain.Assembly).Distinct());

                cb.ConfigureApplicationParts(apm =>
                {
                    apm.AddFrameworkPart(GetType().Assembly);

                    foreach (var part in parts)
                        apm.AddApplicationPart(part);

                    apm.AddFromAppDomain()
                       .WithCodeGeneration();
                });

                return cb.Build();
            }
        }

        /// <summary>
        /// Disconnects this instance of client actor system from cluster
        /// </summary>
        /// <param name="force">Set to <c>true</c> to disconnect ungracefully</param>
        public async Task Disconnect(bool force = false)
        {
            if (!Connected)
                return;

            if (force)
            {
                Client.Abort();
                return;
            }

            await Client.Close().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose() => Client?.Dispose();
    }
}