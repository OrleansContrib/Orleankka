





using System;

using Orleans.Placement;
using Orleans.Concurrency;

namespace Orleankka.Core
{
    namespace Hardcore
    {
        namespace Actor.AutoPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Actor.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[PreferLocalPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[ActivationCountBasedPlacement]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Worker.AutoPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}

		namespace Worker.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[StatelessWorker]
			public class ActorEndpoint : Core.ActorEndpoint, IActorEndpoint {}
		}


    }
}
