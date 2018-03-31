using System;

namespace Orleankka
{
    public interface ReceiveResult
    {}

    [Serializable]
    public sealed class Done : ReceiveResult
    {
        Done(){}
        public static readonly Done Result = new Done();
    }

    [Serializable]
    public sealed class Unhandled : ReceiveResult
    {
        Unhandled(){}
        public static readonly Unhandled Result = new Unhandled();
    }
}