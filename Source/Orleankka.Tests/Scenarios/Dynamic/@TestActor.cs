using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Scenarios.Dynamic
{
    public class SetText : Command
    {
        public readonly string Text;

        public SetText(string text)
        {
            Text = text;
        }
    }

    public class GetText : Query<string>
    {}

    public class TextChanged : Event
    {
        public readonly string Text;

        public TextChanged(string text)
        {
            Text = text;
        }
    }

    public class Throw : Command
    {
        public readonly Exception Exception;

        public Throw(Exception exception)
        {
            Exception = exception;
        }
    }

    public class Attach : Command
    {
        public ActorPath Observer;

        public Attach(ActorPath observer)
        {
            Observer = observer;
        }
    }

    public class TestActor : DynamicActor
    {
        readonly IActorObserverCollection observers;
        string text = "";

        public TestActor()
        {
            observers = new ActorObserverCollection(()=> this);
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
            observers.Notify(new TextChanged(cmd.Text));
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
