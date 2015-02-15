
using System;
using System.Threading.Tasks;

using Orleans;
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
			public interface IActorEndpoint : IGrainWithStringKey, IRemindable
			{
				Task ReceiveTell(RequestEnvelope envelope);
				Task <ResponseEnvelope> ReceiveAsk(RequestEnvelope envelope);
			}
		}

		namespace Singleton.DefaultPlacement.ConcurrencyReentrant.OrderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencyTellInterleave.OrderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencyAskInterleave.OrderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencySequential.UnorderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencyReentrant.UnorderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencyTellInterleave.UnorderedDelivery
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

		namespace Singleton.DefaultPlacement.ConcurrencyAskInterleave.UnorderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencySequential.OrderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyReentrant.OrderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyTellInterleave.OrderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyAskInterleave.OrderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencySequential.UnorderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyReentrant.UnorderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyTellInterleave.UnorderedDelivery
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

		namespace Singleton.RandomPlacement.ConcurrencyAskInterleave.UnorderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencySequential.OrderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyReentrant.OrderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyTellInterleave.OrderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyAskInterleave.OrderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencySequential.UnorderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyReentrant.UnorderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyTellInterleave.UnorderedDelivery
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

		namespace Singleton.PreferLocalPlacement.ConcurrencyAskInterleave.UnorderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencySequential.OrderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyReentrant.OrderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyTellInterleave.OrderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyAskInterleave.OrderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencySequential.UnorderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyReentrant.UnorderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyTellInterleave.UnorderedDelivery
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

		namespace Singleton.ActivationCountBasedPlacement.ConcurrencyAskInterleave.UnorderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencySequential.OrderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyReentrant.OrderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyTellInterleave.OrderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyAskInterleave.OrderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencySequential.UnorderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyReentrant.UnorderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyTellInterleave.UnorderedDelivery
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

		namespace StatelessWorker.DefaultPlacement.ConcurrencyAskInterleave.UnorderedDelivery
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

    }
}
