using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    public abstract class CqsActor : ActorGrain
    {
        public override Task<object> OnReceive(object message)
        {
            switch (message)
            {
                case Command cmd:
                    return HandleCommand(cmd);
                case Query query:
                    return HandleQuery(query);
            }

            throw new InvalidOperationException("Unknown message type: " + message.GetType());
        }

        protected abstract Task<object> HandleCommand(Command cmd);
        protected abstract Task<object> HandleQuery(Query query);
    }

    public abstract class EventSourcedActor : CqsActor
    {
        StreamRef stream;

        public override Task OnActivate()
        {
            stream = System.StreamOf("sms", $"{GetType().Name}-{Id}");
            return base.OnActivate();
        }

        protected override Task<object> HandleQuery(Query query)
        {
            return Dispatch(query);
        }

        protected override async Task<object> HandleCommand(Command cmd)
        {
            var events = await Dispatch<IEnumerable<Event>>(cmd);

            foreach (var @event in events)
            {
                await Dispatch(@event);
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