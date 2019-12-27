using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    public abstract class EventSourcedActor : DispatchActorGrain
    {
        StreamRef<IEventEnvelope> stream;

        public override Task<object> Receive(object message)
        {
            switch (message)
            {
                case Activate _:
                    stream = System.StreamOf<IEventEnvelope>("sms", $"{GetType().Name}-{Id}");
                    return Result(Done);

                case Command cmd:
                    return HandleCommand(cmd);
                
                case Query query:
                    return HandleQuery(query);
                    
                default:
                    return base.Receive(message);
            }
        }

        Task<object> HandleQuery(Query query) => Result(Dispatcher.DispatchResult(this, query));

        async Task<object> HandleCommand(Command cmd)
        {
            var events = Dispatcher.DispatchResult<IEnumerable<Event>>(this, cmd);

            foreach (var @event in events)
            {
                Dispatcher.Dispatch(this, @event);
                await Project(@event);
            }

            return events;
        }

        Task Project(Event @event)
        {
            var envelope = Wrap(@event);
            return stream.Publish(envelope);
        }

        IEventEnvelope Wrap(Event @event)
        {
            var envelopeType = typeof(EventEnvelope<>).MakeGenericType(@event.GetType());
            return (IEventEnvelope) Activator.CreateInstance(envelopeType, Id, @event);
        }
    }
}