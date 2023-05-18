using System;
using Orleans;

namespace Orleankka
{
    public interface LifecycleMessage
    {}

    [Serializable, Immutable]
    public sealed class Activate : LifecycleMessage
    {
        public static readonly Activate External = new Activate();
        public static readonly Activate State = new Activate();
    }

    [Serializable, Immutable]
    public sealed class Deactivate : LifecycleMessage
    {
        public Deactivate(DeactivationReason reason = default) => Reason = reason;

        public DeactivationReason Reason { get; }

        public static readonly Deactivate External = new(new DeactivationReason(DeactivationReasonCode.ApplicationRequested, "Triggered externally"));
        public static readonly Deactivate State = new(new DeactivationReason(DeactivationReasonCode.ApplicationRequested, "State transition"));
    }
}