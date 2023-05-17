using System;

using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleankka
{
    using Orleans.Serialization.Invocation;

    public static class RequestBaseExtensions
    {
        public static bool Message(this IInvokable request, Func<object, bool> predicate) => 
            predicate(request.Message());

        public static bool Message<T>(this IInvokable request, Func<T, bool> predicate) => 
            request.Message() is T m && predicate(m);

        public static object Message(this IInvokable request)
        {
            if (request == null || request.GetArgumentCount() == 0)
                return null;

            var receiveMessage = request.GetArgumentCount() == 1;
            if (receiveMessage)
                return UnwrapImmutable(request.GetArgument(0));

            var streamMessage = request.GetArgumentCount() == 5;
            return streamMessage ? UnwrapImmutable(request.GetArgument(2)) : null;
        }

        static object UnwrapImmutable(object item) => 
            item is Immutable<object> immutable ? immutable.Value : item;
    }
}