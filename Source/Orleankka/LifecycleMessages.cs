using System;
using Orleans;

namespace Orleankka
{
    public interface LifecycleMessage
    {}

    [Serializable, Immutable]
    public sealed class Activate : LifecycleMessage
    {
        public static readonly Activate Message = new Activate();
    }

    [Serializable, Immutable]
    public sealed class Deactivate : LifecycleMessage
    {
        public static readonly Deactivate Message = new Deactivate();
    }
}