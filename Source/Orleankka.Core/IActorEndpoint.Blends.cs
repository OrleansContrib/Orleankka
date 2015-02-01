
using System;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Orleankka.Core
{
	namespace Hardcore
    {
        namespace Singleton
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

		namespace Singleton.Default.Default.Default.BothInterleave
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

		namespace Singleton.Default.Default.Default.TellInterleave
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

		namespace Singleton.Default.Default.Default.AskInterleave
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

		namespace Singleton.Default.Default.Unordered
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

		namespace Singleton.Default.Default.Unordered.BothInterleave
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

		namespace Singleton.Default.Default.Unordered.TellInterleave
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

		namespace Singleton.Default.Default.Unordered.AskInterleave
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

		namespace Singleton.Default.RandomPlacement
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

		namespace Singleton.Default.RandomPlacement.Default.BothInterleave
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

		namespace Singleton.Default.RandomPlacement.Default.TellInterleave
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

		namespace Singleton.Default.RandomPlacement.Default.AskInterleave
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

		namespace Singleton.Default.RandomPlacement.Unordered
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

		namespace Singleton.Default.RandomPlacement.Unordered.BothInterleave
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

		namespace Singleton.Default.RandomPlacement.Unordered.TellInterleave
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

		namespace Singleton.Default.RandomPlacement.Unordered.AskInterleave
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

		namespace Singleton.Default.PreferLocalPlacement
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

		namespace Singleton.Default.PreferLocalPlacement.Default.BothInterleave
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

		namespace Singleton.Default.PreferLocalPlacement.Default.TellInterleave
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

		namespace Singleton.Default.PreferLocalPlacement.Default.AskInterleave
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

		namespace Singleton.Default.PreferLocalPlacement.Unordered
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

		namespace Singleton.Default.PreferLocalPlacement.Unordered.BothInterleave
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

		namespace Singleton.Default.PreferLocalPlacement.Unordered.TellInterleave
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

		namespace Singleton.Default.PreferLocalPlacement.Unordered.AskInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Default.BothInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Default.TellInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Default.AskInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Unordered
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Unordered.BothInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Unordered.TellInterleave
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

		namespace Singleton.Default.ActivationCountBasedPlacement.Unordered.AskInterleave
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

		namespace Singleton.Reentrant
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

		namespace Singleton.Reentrant.Default.Unordered
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

		namespace Singleton.Reentrant.RandomPlacement
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

		namespace Singleton.Reentrant.RandomPlacement.Unordered
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

		namespace Singleton.Reentrant.PreferLocalPlacement
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

		namespace Singleton.Reentrant.PreferLocalPlacement.Unordered
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

		namespace Singleton.Reentrant.ActivationCountBasedPlacement
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

		namespace Singleton.Reentrant.ActivationCountBasedPlacement.Unordered
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

		namespace StatelessWorker
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

		namespace StatelessWorker.Default.Default.Default.BothInterleave
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

		namespace StatelessWorker.Default.Default.Default.TellInterleave
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

		namespace StatelessWorker.Default.Default.Default.AskInterleave
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

		namespace StatelessWorker.Default.Default.Unordered
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

		namespace StatelessWorker.Default.Default.Unordered.BothInterleave
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

		namespace StatelessWorker.Default.Default.Unordered.TellInterleave
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

		namespace StatelessWorker.Default.Default.Unordered.AskInterleave
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

		namespace StatelessWorker.Reentrant
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

		namespace StatelessWorker.Reentrant.Default.Unordered
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

    }
}
