using System;

namespace Orleankka
{
    using Utility;

    [AttributeUsage(AttributeTargets.Class)]
    public class ActorAttribute : Attribute
    {
        public Placement Placement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class WorkerAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ActorTypeCodeAttribute : Attribute
    {
        internal readonly string Code;

        public ActorTypeCodeAttribute(string code)
        {
            Requires.NotNullOrWhitespace(code, nameof(code));

            if (code.Contains(ActorPath.Separator[0]))
                throw new ArgumentException($"Actor type code cannot contain path separator: {code}");

            Code = code;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ReentrantAttribute : Attribute
    {
        internal readonly Type Message;

        public ReentrantAttribute(Type message)
        {
            Requires.NotNull(message, nameof(message));
            Message = message;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class KeepAliveAttribute : Attribute
    {
        public double Minutes;
        public double Hours;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StreamSubscriptionAttribute : Attribute
    {
        public string Source;
        public string Target;
        public string Filter;
    }
}