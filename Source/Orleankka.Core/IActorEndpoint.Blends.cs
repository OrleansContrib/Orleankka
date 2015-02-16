
using System;
using System.Threading.Tasks;

using Orleans;
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
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyAskInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyTellInterleave.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencySequential.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyReentrant.OrderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyAskInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyTellInterleave.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencySequential.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.PreferLocalPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Worker.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Actor.EvenDistributionPlacement.ConcurrencyReentrant.UnorderedDelivery
		{
			/// <summary> 
			/// FOR INTERNAL USE ONLY!
			/// </summary>
			[Unordered]
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				[AlwaysInterleave]
				Task ReceiveTell(RequestEnvelope envelope);
				[AlwaysInterleave]
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

    }
}
