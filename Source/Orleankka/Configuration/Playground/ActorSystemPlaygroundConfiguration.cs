using System;
using System.Linq;

using Orleans.Runtime.Configuration;
using Orleankka.Configuration.Embedded;

namespace Orleankka.Configuration.Playground
{
    public sealed class ActorSystemPlaygroundConfiguration : ActorSystemEmbeddedConfiguration
    {
        internal ActorSystemPlaygroundConfiguration()
        {
            cluster = new ClusterConfiguration()
                .LoadFromEmbeddedResource<ActorSystemPlaygroundConfiguration>("Cluster.Configuration.xml");

            client = new ClientConfiguration()
                .LoadFromEmbeddedResource<ActorSystemPlaygroundConfiguration>("Client.Configuration.xml");

            cluster.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.MembershipTableGrain;
            cluster.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.ReminderTableGrain;
        }
    }
}