using System;
using System.Linq;

namespace Orleankka
{
    enum Activation
    {
        Actor,
        Worker
    }

    enum Delivery
    {
        Ordered,
        Unordered,
    }

    public enum Placement
    {
        Auto,
        PreferLocal,
        DistributeEvenly
    }
}
