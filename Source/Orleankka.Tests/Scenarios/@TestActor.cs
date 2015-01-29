using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleans;

namespace Orleankka.Scenarios
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

    public class SetReminder : Command
    {}

    public class HasBeenReminded : Query<bool>
    {}

    public class GetInstanceHashcode : Query<int>
    {}

    public class TestActor : Actor
    {
        readonly IObserverCollection observers;
        readonly IActivationService activation;
        readonly IReminderService reminders;

        string text = "";
        bool reminded;

        public TestActor()
        {
            observers  = new ObserverCollection(this);
            activation = new ActivationService(this);
            reminders  = new ReminderService(this);
        }

        public override Task OnTell(object message)
        {
            return this.Handle((dynamic)message);
        }

        public override async Task<object> OnAsk(object message)
        {
            return await this.Answer((dynamic)message);
        }

        public override Task OnReminder(string id)
        {
            reminded = true;
            activation.DelayDeactivation(TimeSpan.FromSeconds(600));
            return TaskDone.Done;
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
        
        public Task Handle(SetReminder cmd)
        {
            reminders.Register("test", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            activation.DeactivateOnIdle();
            return TaskDone.Done;
        }

        public Task<bool> Answer(HasBeenReminded query)
        {
            return Task.FromResult(reminded);
        }

        public Task<int> Answer(GetInstanceHashcode query)
        {
            return Task.FromResult(RuntimeHelpers.GetHashCode(this));
        }
    }
}
