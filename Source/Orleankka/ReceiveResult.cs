using System;

namespace Orleankka
{
    public interface ReceiveResult
    {}

    [Serializable]
    public sealed class Done : ReceiveResult
    {
        public static readonly Done Result = new Done();
    }

    [Serializable]
    public sealed class Unhandled : ReceiveResult
    {
        public static readonly Unhandled Result = new Unhandled();
    }
}