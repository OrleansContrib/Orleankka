using System;
using System.Linq;

namespace Orleankka
{
    public abstract class ObserverRef : Ref
    {
        public abstract void Notify(object message);
    }
}