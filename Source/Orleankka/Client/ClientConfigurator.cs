using System;

using Microsoft.Extensions.DependencyInjection;

using Orleans;
using Orleans.Hosting;
using Orleans.Streams;

namespace Orleankka.Client
{
    using Core;
    using Utility;

    public sealed class ClientConfigurator
    {
        readonly ActorInterfaceRegistry registry =
             new ActorInterfaceRegistry();

        IActorRefInvoker invoker;

        /// <summary>
        /// Registers global <see cref="ActorRef"/> invoker (interceptor)
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        public ClientConfigurator ActorRefInvoker(IActorRefInvoker invoker)
        {
            Requires.NotNull(invoker, nameof(invoker));

            if (this.invoker != null)
                throw new InvalidOperationException("ActorRef invoker has been already registered");

            this.invoker = invoker;
            return this;
        }

        internal void Configure(IClientBuilder builder, IServiceCollection services)
        {
            RegisterAssemblies(builder);
            RegisterActorInterfaces();
            RegisterDependencies(services);
        }

        void RegisterAssemblies(IClientBuilder builder) => 
            registry.Register(builder.GetApplicationPartManager(), x => x.ActorInterfaces());

        void RegisterDependencies(IServiceCollection services)
        {
            services.AddSingleton<IActorSystem>(sp => sp.GetService<ClientActorSystem>());
            services.AddSingleton<IClientActorSystem>(sp => sp.GetService<ClientActorSystem>());

            services.AddSingleton(sp => new ClientActorSystem(
                sp.GetService<IStreamProviderManager>(), 
                sp.GetService<IGrainFactory>(), 
                invoker));
        }

        void RegisterActorInterfaces() => ActorInterface.Register(registry.Mappings);
    }

    public static class ClientBuilderExtension
    {
        public static IClientBuilder ConfigureOrleankka(this IClientBuilder builder) => 
            ConfigureOrleankka(builder, new ClientConfigurator());

        public static IClientBuilder ConfigureOrleankka(this IClientBuilder builder, Func<ClientConfigurator, ClientConfigurator> configure) => 
            ConfigureOrleankka(builder, configure(new ClientConfigurator()));

        public static IClientBuilder ConfigureOrleankka(this IClientBuilder builder, ClientConfigurator cfg) => 
            builder
                .ConfigureServices(services => cfg.Configure(builder, services))
                .ConfigureApplicationParts(apm => apm
                    .AddApplicationPart(typeof(IClientEndpoint).Assembly)
                    .WithCodeGeneration());

        public static IClientActorSystem ActorSystem(this IClusterClient client) => client.ServiceProvider.GetRequiredService<IClientActorSystem>();
    }
}