using System;
using System.Linq;
using System.Runtime.Serialization;

using Orleankka;
using Orleans.Concurrency;

namespace Demo
{
    [Immutable, Serializable]
    public class Search : Query<int>
    {
        public readonly string Subject;

        public Search(string subject)
        {
            Subject = subject;
        }
    }

    [Immutable, Serializable]
    public class MonitorAvailabilityChanges : Command
    {
        public readonly ActorPath Sender;

        public MonitorAvailabilityChanges(ActorPath sender)
        {
            Sender = sender;
        }
    }

    [Immutable, Serializable]
    public class AvailabilityChanged : Event
    {
        public readonly string Api;
        public readonly bool Available;

        public AvailabilityChanged(string api, bool available)
        {
            Api = api;
            Available = available;
        }
    }

    [Immutable, Serializable]
    public class ApiUnavailableException : ApplicationException
    {
        public ApiUnavailableException(string api)
            : base(api + " api is unavailable. Try later!")
        {}

        protected ApiUnavailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}
    }

    public interface IApi : IActor {}
}
