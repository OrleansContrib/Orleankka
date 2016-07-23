using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleankka.CSharp;

namespace Orleankka.TestKit
{
    namespace CSharp
    {
        public static class ActorRefMockExtensions
        {
            public static ActorRefMock<TActor> MockActorOf<TActor>(this ActorSystemMock system, string id) where TActor : IActor
            {
                var path = typeof(TActor).ToActorPath(id);
                return new ActorRefMock<TActor>(system.MockActorOf(path));
            }
        }

        [Serializable]
        public class ActorRefMock<T> : ActorRef<T> where T: IActor
        {
            readonly ActorRefMock @ref;

            public ActorRefMock(ActorRefMock @ref)
                : base(@ref)
            {
                this.@ref = @ref;
            }

            public TellExpectation<TMessage> ExpectTell<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> => 
                @ref.ExpectTell(match);

            public AskExpectation<TMessage> ExpectAsk<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> => 
                @ref.ExpectAsk(match);

            public AskExpectation<TMessage> ExpectAsk<TMessage, TResult>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T, TResult> => 
                @ref.ExpectAsk(match);

            public TellExpectation<TMessage> ExpectNotify<TMessage>(Expression<Func<TMessage, bool>> match = null) where TMessage : ActorMessage<T> =>
                @ref.ExpectNotify(match);

            public override Task Tell(ActorMessage<T> message) => 
                @ref.Tell(message);

            public override Task<TResult> Ask<TResult>(ActorMessage<T> message) => 
                @ref.Ask<TResult>(message);

            public override Task<TResult> Ask<TResult>(ActorMessage<T, TResult> message) => 
                @ref.Ask<TResult>(message);

            public override void Notify(ActorMessage<T> message) => 
                @ref.Notify(message);

            public void Reset() => 
                @ref.Reset();

            public IEnumerable<RecordedMessage> RecordedMessages => 
                @ref.RecordedMessages;
        }
    }
}