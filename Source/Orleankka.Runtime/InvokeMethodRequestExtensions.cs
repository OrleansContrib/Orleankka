using System;

using Orleans.Concurrency;
using Orleans.Runtime;

namespace Orleankka
{
    public static class RequestBaseExtensions
    {
        public static bool Message(this RequestBase request, Func<object, bool> predicate) => 
            predicate(request.Message());

        public static bool Message<T>(this RequestBase request, Func<T, bool> predicate) => 
            request.Message() is T m && predicate(m);

        public static object Message(this RequestBase request)
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