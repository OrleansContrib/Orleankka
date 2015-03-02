using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActorAttribute : Attribute
    {
        public Placement Placement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WorkerAttribute : Attribute
    {}
}
