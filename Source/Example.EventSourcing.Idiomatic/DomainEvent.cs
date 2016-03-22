using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class DomainEvent<T> where T: Event
    {
        public DomainEvent(string sourceId, T @event)
        {
            SourceId = sourceId;
            Event  = @event;
        }
        public  string SourceId { get; }
        public  T Event { get; }
    }
}