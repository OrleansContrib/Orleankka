using System;
using System.Linq;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

namespace Orleankka
{
    public static class InvokeMethodRequestExtensions
    {
        public static bool Any(this InvokeMethodRequest request, params Type[] messages)
        {
            var message = request.Message().GetType();
            return messages.Any(x => x == message);
        }

        public static object Message(this InvokeMethodRequest request)
        {
            if (request?.Arguments == null)
                return null;

            var receiveMessage = request.Arguments.Length == 1;
            if (receiveMessage)
                return UnwrapImmutable(request.Arguments[0]);

            var streamMessage = request.Arguments.Length == 5;
            return streamMessage ? UnwrapImmutable(request.Arguments[2]) : null;
        }

        static object UnwrapImmutable(object item) => 
            item is Immutable<object> ? ((Immutable<object>)item).Value : item;
    }
}