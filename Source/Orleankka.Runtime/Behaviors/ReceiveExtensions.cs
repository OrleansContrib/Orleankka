using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleankka.Behaviors
{
    using Utility;

    public static class ReceiveExtensions
    {
        public static Receive Trait(this Receive receive, params Receive[] traits)
        {
            Requires.NotNull(traits, nameof(traits));
                
            if (traits.Length == 0)
                throw new ArgumentException("no traits were specified", nameof(traits));

            var handlers = new List<Receive>(traits);
            handlers.Insert(0, receive);

            return Handle;

            async Task<object> Handle(object message)
            {
                return message is LifecycleMessage
                           ? await HandleLifecycleMessage()
                           : await HandleReceiveMessage();

                async Task<object> HandleLifecycleMessage()
                {
                    for (var i = handlers.Count - 1; i >= 0; i--)
                    {
                        var handler = handlers[i];
                        await handler(message);
                    }

                    return Done.Result;
                }

                async Task<object> HandleReceiveMessage()
                {
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
}