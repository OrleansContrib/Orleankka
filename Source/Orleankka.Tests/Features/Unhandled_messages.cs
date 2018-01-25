using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleankka.Features
{
    namespace Unhandled_messages
    {
        using Testing;

        public interface IEmptyActor : IActorGrain {}
        public class EmptyActor : ActorGrain, IEmptyActor
        {}

        public interface ICallBaseReceiveActor : IActorGrain {}
        public class CallBaseReceiveActor : ActorGrain, ICallBaseReceiveActor
        {
            protected override Task<object> Receive(object message) => 
                message is null ? null : base.Receive(message);
        }        
        
        public interface IReceiveReturnsUnhandledActor : IActorGrain {}
        public class ReceiveReturnsUnhandledActor : ActorGrain, IReceiveReturnsUnhandledActor
        {
            protected override Task<object> Receive(object message) => Result(Unhandled);
        }
        
        [TestFixture]
        [RequiresSilo]
        public class Tests
        {
            [Test]
            public void When_empty()
            {
                var actor = TestActorSystem.Instance.FreshActorOf<EmptyActor>();
                Assert.ThrowsAsync<UnhandledMessageException>(async ()=> 
                    await actor.Tell("foo"));
            }            
            
            [Test]
            public void When_calls_base_receive()
            {
                var actor = TestActorSystem.Instance.FreshActorOf<CallBaseReceiveActor>();
                Assert.ThrowsAsync<UnhandledMessageException>(async ()=> 
                    await actor.Tell("foo"));
            }
            
            [Test]
            public void When_receive_returns_Unhandled()
            {
                var actor = TestActorSystem.Instance.FreshActorOf<ReceiveReturnsUnhandledActor>();
                Assert.ThrowsAsync<UnhandledMessageException>(async ()=> 
                    await actor.Tell("foo"));
            }
        }
    }
}