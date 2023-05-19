using System;

namespace Orleankka
{
    using Orleans;

    public interface ReceiveResult
    {}

    [Serializable, GenerateSerializer]
    public sealed class Done : ReceiveResult
    {
        Done(){}
        public static readonly Done Result = new Done();
    }

    [Serializable, GenerateSerializer]
    public sealed class Unhandled : ReceiveResult
    {
        Unhandled(){}
        public static readonly Unhandled Result = new Unhandled();
    }
}