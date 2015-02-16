using System;

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
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Actor.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Actor.PreferLocalPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Actor.EvenDistributionPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Worker.AutoPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

		namespace Worker.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : Core.IActorEndpoint {}
		}

    }
}
