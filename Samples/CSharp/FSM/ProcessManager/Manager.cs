using System.Collections.Generic;
using System.Threading.Tasks;

using Orleankka;

namespace ProcessManager
{
    class ManagerState
    {
        public readonly IList<JobState> Jobs = new List<JobState>();
    }

    class JobState
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Previous { get; set; }
        public double Progress { get; set; }
    }

    interface IManager : IActorGrain
    { }

    class Manager : ActorGrain, IManager
    {
        public override Task<object> Receive(object message)
        {
            throw new System.NotImplementedException();
        }
    }
}