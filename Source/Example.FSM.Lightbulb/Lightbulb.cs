using System;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    [Serializable] public class PressSwitch {}
    [Serializable] public class Touch {}
    [Serializable] public class HitWithHammer {}

    public interface ILightbulb : IActorGrain
    {}

    public class Lightbulb : ActorGrain, ILightbulb
    {
        readonly Behavior behavior;
        bool smashed;

        public Lightbulb()
        {
            behavior = new Behavior();
            behavior.Become(Off);
        }

        public override Task<object> Receive(object message)
        {
            // any "global" message handling here
            switch (message)
            {
                case HitWithHammer _:
                    smashed = true;
                    return Result("Smashed!");
                case PressSwitch _ when smashed:
                    return Result("Broken");
                case Touch _ when smashed:
                    return Result("OW!");
            }

            // if not handled, use behavior specific
            return behavior.Receive(message);
        }

        Task<object> Off(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    behavior.Become(On);
                    return Result("Turning on");
                case Touch _:
                    return Result("Cold");
            }
            
            return Done;
        }
        
        Task<object> On(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    behavior.Become(Off);
                    return Result("Turning off");
                case Touch _:
                    return Result("Hot!");
            }
            
            return Done;
        }

    }

    class Behavior
    {
        public Task<object> Receive(object message)
        {
            throw new NotImplementedException();
        }

        public void Become(Func<object, Task<object>> behavior)
        {
            throw new NotImplementedException();
        }
    }
}