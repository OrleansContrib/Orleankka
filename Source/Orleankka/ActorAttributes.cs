using System;
using System.Linq;

namespace Orleankka
{
    using Utility;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActorAttribute : Attribute
    {
        public Placement Placement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class WorkerAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class InterleaveAttribute : Attribute
    {
        internal readonly Type Message;

        public InterleaveAttribute(Type message)
        {
            Requires.NotNull(message, "message");
            Message = message;
        }
    }
}
