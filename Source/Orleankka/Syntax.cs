using System;
using System.Threading.Tasks;

namespace Orleankka
{
    using Meta;

    public static class Syntax
    {
        public static MessageResult<T> result<T>(Message<T> message) => new(message);
    }

    public readonly struct MessageResult<T>
    {
        readonly Message<T> message;
        public MessageResult(Message<T> message) => this.message = message;

        public static Task<T> operator <(MessageResult<T> result, ActorRef @ref) => throw new NotImplementedException();
        public static Task<T> operator >(MessageResult<T> result, ActorRef @ref) => @ref.Ask<T>(result.message);
    }
}