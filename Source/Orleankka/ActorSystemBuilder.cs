using System;
using System.Linq;

using Orleankka.Configuration.Embedded;
using Orleankka.Configuration.Playground;

namespace Orleankka
{
    public partial class ActorSystem
    {
        public static ActorSystemConfiguration Configure()
        {
            return new ActorSystemConfiguration();
        }
    }

    public sealed class ActorSystemConfiguration
    {
        internal ActorSystemConfiguration()
        {}

        public ActorSystemClientConfiguration Client()
        {
            return new ActorSystemClientConfiguration();
        }

        public ActorSystemClusterConfiguration Cluster()
        {
            return new ActorSystemClusterConfiguration();
        }

        public ActorSystemEmbeddedConfiguration Embedded()
        {
            return new ActorSystemEmbeddedConfiguration();
        }

        public ActorSystemPlaygroundConfiguration Playground()
        {
            return new ActorSystemPlaygroundConfiguration();
        }
    }

    public sealed class ActorSystemClientConfiguration
    {
        internal ActorSystemClientConfiguration()
        {}
    }

    public sealed class ActorSystemClusterConfiguration
    {
        internal ActorSystemClusterConfiguration()
        {}
    }
}