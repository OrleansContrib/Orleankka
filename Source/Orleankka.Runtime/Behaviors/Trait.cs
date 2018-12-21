using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public static class Trait
    {
        public static Receive[] Of(params Receive[] behaviors) => behaviors;

        internal static Receive Join(this Receive behavior, params Receive[] traits)
        {
            Requires.NotNull(behavior, nameof(behavior));
            Requires.NotNull(traits, nameof(traits));

            if (traits.Length == 0)
                return behavior;

            var handlers = new List<Receive>(traits);
            handlers.Insert(0, behavior);

            return Handle;

            async Task<object> Handle(object message)
            {
                if (message is LifecycleMessage)
                    return Unhandled.Result;

                foreach (var handler in handlers)
                {
                    var result = await handler(message);
                    if (result != Unhandled.Result)
                        return result;
                }

                return Unhandled.Result;
            }
        }
    }
}