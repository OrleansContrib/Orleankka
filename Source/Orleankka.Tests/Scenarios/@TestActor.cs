using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Orleankka.Services;

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
        public readonly ObserverRef Observer;

        public Attach(ObserverRef observer)
        {
            Observer = observer;
        }
    }

    public class Detach : Command
    {
        public readonly ObserverRef Observer;

        public Detach(ObserverRef observer)
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

        public override Task<object> OnReceive(object message)
        {
            return this.Handle((dynamic)message);
        }

        public override Task OnReminder(string id)
        {
            reminded = true;
            activation.DelayDeactivation(TimeSpan.FromSeconds(600));
            return Done();
        }

        public Task<object> Handle(SetText cmd)
        {
            text = cmd.Text;
            observers.Notify(new TextChanged(cmd.Text));
            return Done();
        }

        public Task<object> Handle(GetText q)
        {
            return Result(text);
        }

        public Task<object> Handle(Attach cmd)
        {
            observers.Add(cmd.Observer);
            return Done();
        }
        
        public Task<object> Handle(Detach cmd)
        {
            observers.Remove(cmd.Observer);
            return Done();
        }

        public Task<object> Handle(Throw cmd)
        {
            throw cmd.Exception;
        }

        public Task<object> Handle(SetReminder cmd)
        {
            reminders.Register("test", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            activation.DeactivateOnIdle();
            return Done();
        }

        public Task<object> Handle(HasBeenReminded q)
        {
            return Result(reminded);
        }

        public Task<object> Answer(GetInstanceHashcode q)
        {
            return Result(RuntimeHelpers.GetHashCode(this));
        }
    }
}
