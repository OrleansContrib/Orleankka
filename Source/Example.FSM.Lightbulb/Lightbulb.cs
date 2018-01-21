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
        bool smashed;

        public Lightbulb()
        {
            Behavior.Initial(Off);
        }

        public override async Task<object> Receive(object message)
        {
            // any "global" message handling here
            switch (message)
            {
                case HitWithHammer _:
                    smashed = true;
                    return "Smashed!";
                case PressSwitch _ when smashed:
                    return "Broken";
                case Touch _ when smashed:
                    return "OW!";
            }

            // if not handled, use behavior specific
            return await base.Receive(message);
        }

        [Behavior] async Task<object> Off(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    await Behavior.Become(On);
                    return "Turning on";
                case Touch _:
                    return "Cold";
                default:
                    return Unhandled;
            }
        }

        [Behavior] async Task<object> On(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    await Behavior.Become(Off);
                    return "Turning off";
                case Touch _:
                    return "Hot!";
                default:
                    return Unhandled;
            }
        }

    }
}