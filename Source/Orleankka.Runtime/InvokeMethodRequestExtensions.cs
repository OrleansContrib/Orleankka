using System;

using Orleans.CodeGeneration;
using Orleans.Concurrency;

namespace Orleankka
{
    public static class InvokeMethodRequestExtensions
    {
        public static bool Message(this InvokeMethodRequest request, Func<object, bool> predicate) => 
            predicate(request.Message());

        public static bool Message<T>(this InvokeMethodRequest request, Func<T, bool> predicate) => 
            request.Message() is T m && predicate(m);

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
            item is Immutable<object> immutable ? immutable.Value : item;
    }
}