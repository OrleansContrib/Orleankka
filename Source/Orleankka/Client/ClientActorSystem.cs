using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Runtime.Configuration;

namespace Orleankka.Client
{
    using Core;

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
        internal ClientActorSystem(ClientConfiguration configuration)
        {
            Client = new ClientBuilder()
                .UseConfiguration(configuration)
                .ConfigureServices(services =>
                {
                    services.Add(ServiceDescriptor.Singleton<IActorSystem>(this));
                    services.Add(ServiceDescriptor.Singleton<IClientActorSystem>(this));
                })
                .Build();

            Initialize(Client.ServiceProvider);
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
        public IClusterClient Client { get; }

        /// <summary>
        /// Checks whether this client has been successfully connected (ie initialized)
        /// </summary>
        public bool Connected => Client.IsInitialized;

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
                    await Client.Connect();
                }
                catch (Exception ex)
                {
                    if (retries >= 0)
                    {
                        Trace.TraceWarning($"Can't connect to cluster. Trying again in {(int)retryTimeout.Value.TotalSeconds} seconds ... Got error: /n{ex}");
                        Thread.Sleep(retryTimeout.Value);
                    }
                    else
                    {
                        Trace.TraceError($"Can't connect to cluster. Max retries reached. Got error: /n{ex}");
                        throw;
                    }
                }
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

            await Client.Close();
        }

        /// <inheritdoc />
        public void Dispose() => Client?.Dispose();
    }
}