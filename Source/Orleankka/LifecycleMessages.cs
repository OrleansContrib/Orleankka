using System;
using Orleans;

namespace Orleankka
{
    public interface LifecycleMessage
    {}

    [GenerateSerializer, Immutable]
    public sealed class Activate : LifecycleMessage
    {
        public static readonly Activate External = new Activate();
        public static readonly Activate State = new Activate();
    }

    [GenerateSerializer, Immutable]
    public sealed class Deactivate : LifecycleMessage
    {
        public Deactivate(DeactivationReason reason = default) => Reason = reason;

        [Id(0)]
        public DeactivationReason Reason { get; }

        public static readonly Deactivate External = new(new DeactivationReason(DeactivationReasonCode.ApplicationRequested, "Triggered externally"));
        public static readonly Deactivate State = new(new DeactivationReason(DeactivationReasonCode.ApplicationRequested, "State transition"));
    }
}