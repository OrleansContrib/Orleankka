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
    public class ReentrantAttribute : Attribute
    {
        internal readonly Type Message;

        public ReentrantAttribute(Type message)
        {
            Requires.NotNull(message, "message");
            Message = message;
        }
    }
}
