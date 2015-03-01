using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    public interface Command
    {}

    public interface Query
    {}

    public interface Query<TResult> : Query
    {}

    public interface Event
    {}

    public static class RefExtensions
    {
        public static Task Send<TCommand>(this ActorRef @ref, TCommand cmd) where TCommand : Command
        {
            return @ref.Tell(cmd);
        }

        public static Task<TResult> Query<TResult>(this ActorRef @ref, Query<TResult> query)
        {
            return @ref.Ask<TResult>(query);
        }

        public static void Publish<TEvent>(this ObserverRef @ref, TEvent @event) where TEvent : Event
        {
            @ref.Notify(@event);
        }
    }

    public abstract class CqsActor : Actor
    {
        protected override void Define()
        {
            Reentrant(req => req is Query);
        }

        public override Task<object> OnReceive(object message)
        {
            var cmd = message as Command;
            if (cmd != null)
                return HandleCommand(cmd);

            var query = message as Query;
            if (query != null)
                return HandleQuery(query);

            throw new InvalidOperationException("Unknown message type: " + message.GetType());
        }

        protected abstract Task<object> HandleCommand(Command cmd);
        protected abstract Task<object> HandleQuery(Query query);
    }

    public abstract class EventSourcedActor : CqsActor
    {
        protected override Task<object> HandleCommand(Command cmd)
        {
            var events = DispatchResult<IEnumerable<Event>>(cmd).ToArray();

            foreach (var @event in events)
                Dispatch(@event);

            return Task.FromResult((object)events);
        }

        protected override Task<object> HandleQuery(Query query)
        {
            return Task.FromResult(DispatchResult(query));
        }
    }
}