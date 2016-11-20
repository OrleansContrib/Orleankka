using System;
using System.Linq;

namespace Orleankka
{
    public interface ActorMessage<TActor> where TActor : IActor
    {}

    public interface ActorMessage<TActor, TResult> where TActor : IActor
    {}
}