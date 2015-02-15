
using System;

using Orleans.Placement;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    namespace Hardcore
    {
        namespace Singleton.DefaultPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[RandomPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.PreferLocalPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace StatelessWorker.DefaultPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

    }
}
