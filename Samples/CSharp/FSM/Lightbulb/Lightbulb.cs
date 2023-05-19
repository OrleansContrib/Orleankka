using System;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Behaviors;

using Orleans;

namespace Example
{
    [Serializable, GenerateSerializer] public class PressSwitch {}
    [Serializable, GenerateSerializer] public class Touch {}
    [Serializable, GenerateSerializer] public class HitWithHammer {}
    [Serializable, GenerateSerializer] public class Fix {}

    public interface ILightbulb : IActorGrain, IGrainWithStringKey {}

    public class Lightbulb : ActorGrain, ILightbulb
    {
        readonly Behavior behavior;

        public Lightbulb()
        {
            behavior = new Behavior();
            behavior.Initial(Off);
        }

        public override async Task<object> Receive(object message)
        {
            var result = await DoReceive(message);
            
            if (behavior.Previous != behavior.Current)
                await SaveState();

            return result;
        }

        static Task SaveState() => Task.CompletedTask;
        Task LoadState() => Task.CompletedTask;

        async Task<object> DoReceive(object message)
        {
            // any "global" message handling here
            switch (message)
            {
                case Activate _ : 
                    await LoadState();
                    return Done;
                case HitWithHammer _ when behavior.Current.Name != nameof(Smashed):
                    await behavior.BecomeStacked(Smashed);
                    return "Smashed!";
                case Deactivate _:
                    Console.WriteLine("Deactivated");
                    await Task.Delay(2000);
                    return "";
            }

            // if not handled, use behavior specific
            return await behavior.Receive(message);
        }

        async Task<object> Smashed(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    return "Broken";
                case Touch _:
                    return "OW!";
                case  Fix _:
                    await behavior.Unbecome();
                    return "Fixed";
                default:
                    return Unhandled;
            }
        }

        async Task<object> Off(object message)
        {
            switch (message)
            {
                case PressSwitch _:
                    await behavior.Become(On);
                    return "Turning on";
                case Touch _:
                    return "Cold";
                default:
                    return Unhandled;
            }
        }

        async Task<object> On(object message)
        {
            switch (message)
            {
                case Reminder _: 
                    Console.WriteLine("Reminded");
                    return Done;
                case Activate _: 
                    await NotifyLightsOn();
                    return "";
                case Deactivate _: 
                    await CleanupOn();
                    return "";
                case PressSwitch _:
                    await behavior.Become(Off);
                    return "Turning off";
                case Touch _:
                    return "Hot!";
                default:
                    return Unhandled;
            }

            async Task NotifyLightsOn()
            {
                await Reminders.Register("123", TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));

                Timers.Register("123", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), () =>
                {
                    Console.WriteLine("Lights, on!!!");
                    return Task.CompletedTask;
                });
            }

            async Task CleanupOn()
            {
                Timers.Unregister("123");

                await Reminders.Unregister("123");
            }
        }
    }
}