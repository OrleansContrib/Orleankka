using System;
using System.Linq;

namespace Orleankka
{
    public enum ConcurrencyKind
    {
        Sequential,
        Reentrant,
        TellInterleave,
        AskInterleave,
    }
}
