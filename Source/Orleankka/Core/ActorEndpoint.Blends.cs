
using System;

using Orleans.Placement;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    namespace Hardcore
    {
        namespace Actor.PreferLocalPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Worker.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : ActorEndpointBase, IActorEndpoint {}
		}

    }
}
