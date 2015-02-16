
using System;
using System.Threading.Tasks;

using Orleans;
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
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Actor.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Worker.AutoPlacement.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

		namespace Worker.AutoPlacement.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task<ResponseEnvelope> Receive(RequestEnvelope envelope);
			}
		}

    }
}
