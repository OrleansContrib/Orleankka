using System;

using Orleans;
using Orleans.Core;
using Orleans.Runtime;

namespace Orleankka.Core
{
    public static class ActorExtensions
    {
        public static IServiceProvider ServiceProvider(this Actor actor) => actor.Host.ServiceProvider;
        public static IGrainFactory GrainFactory(this Actor actor) => actor.Host.GrainFactory;
        public static IGrainIdentity Identity(this Actor actor) => actor.Host.Identity;
        public static string IdentityString(this Actor actor) => actor.Host.IdentityString;
        public static Logger Logger(this Actor actor) => actor.Host.Logger();
    }
}
