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
        Auto,
        PreferLocal,
        DistributeEvenly
    }

    public enum Delivery
    {
        Ordered,
        Unordered,
    }
}
