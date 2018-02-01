using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    public abstract class EventSourcedActor : DispatchActorGrain
    {
        StreamRef stream;

        public override Task<object> Receive(object message)
        {
            switch (message)
            {
                case Activate _:
                    stream = System.StreamOf("sms", $"{GetType().Name}-{Id}");
                    return Result(Done);

                case Command cmd:
                    return HandleCommand(cmd);
                
                case Query query:
                    return HandleQuery(query);
                    
                default:
                    return base.Receive(message);
            }
        }

        Task<object> HandleQuery(Query query) => Result((dynamic)this).Handle((dynamic)query);

        async Task<object> HandleCommand(Command cmd)
        {
            var events = (IEnumerable<Event>)((dynamic)this).Handle((dynamic)cmd);

            foreach (var @event in events)
            {
                ((dynamic)this).On((dynamic)@event);
                await Project(@event);
            }

            return events;
        }

        Task Project(Event @event)
        {
            var envelope = Wrap(@event);
            return stream.Push(envelope);
        }

        object Wrap(Event @event)
        {
            var envelopeType = typeof(EventEnvelope<>).MakeGenericType(@event.GetType());
            return Activator.CreateInstance(envelopeType, Id, @event);
        }
    }
}