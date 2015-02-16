using System;
using System.Linq;

namespace Orleankka
{
    enum Activation
    {
        Actor,
        Worker
    }

    public enum Placement
    {
        Random,
        PreferLocal,
        DistributeEvenly
    }

    public enum Concurrency
    {
        Sequential,
        Reentrant,
        TellInterleave,
        AskInterleave,
    }

    public enum Delivery
    {
        Ordered,
        Unordered,
    }
}
