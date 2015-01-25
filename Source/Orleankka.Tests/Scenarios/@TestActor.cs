using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Scenarios
{
    public class TestActor : Actor, ITestActor
    {
        readonly IActorObserverCollection observers;
        string text = "";

        public TestActor()
        {
            observers = new ActorObserverCollection(()=> Self);
        }

        public override Task OnTell(object message)
        {
            return this.Handle((dynamic)message);
        }

        public override async Task<object> OnAsk(object message)
        {
            return await this.Answer((dynamic)message);
        }

        public Task Handle(SetText cmd)
        {
            text = cmd.Text;
            observers.Notify(cmd.Text);
            return TaskDone.Done;
        }

        public Task<string> Answer(GetText query)
        {
            return Task.FromResult(text);
        }

        public Task Handle(Attach cmd)
        {
            observers.Add(ObserverOf(cmd.Observer));
            return TaskDone.Done;
        }

        public Task Handle(Throw cmd)
        {
            throw cmd.Exception;
        }
    }
}
