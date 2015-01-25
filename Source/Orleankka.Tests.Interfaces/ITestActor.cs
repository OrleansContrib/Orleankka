using System;
using System.Linq;

using Orleans.Concurrency;

namespace Orleankka
{
    [Immutable, Serializable]
    public class SetText : Command
    {
        public readonly string Text;

        public SetText(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class GetText : Query<string> 
    {}

    [Immutable, Serializable]
    public class Throw : Command
    {
        public readonly Exception Exception;

        public Throw(Exception exception)
        {
            Exception = exception;
        }
    }

    [Immutable, Serializable]
    public class Attach : Command
    {
        public ActorPath Observer;

        public Attach(ActorPath observer)
        {
            Observer = observer;
        }
    }

    public interface ITestActor : IActor {}
}