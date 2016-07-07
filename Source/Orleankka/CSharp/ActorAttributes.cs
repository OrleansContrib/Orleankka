using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Orleankka.Utility;

namespace Orleankka.CSharp
{
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
        internal static Func<object, bool> Predicate(Type actor)
        {
            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true).ToArray();

            if (attributes.Length == 0)
                return message => false;

            var messages = new HashSet<Type>();

            foreach (var attribute in attributes)
            {
                if (messages.Contains(attribute.Message))
                    throw new InvalidOperationException(
                        $"{attribute.Message} was already registered as Reentrant for {actor}");

                messages.Add(attribute.Message);
            }

            return (message) => messages.Contains(message.GetType());
        }

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
        internal static TimeSpan Timeout(Type actor)
        {
            var attribute = actor.GetCustomAttribute<KeepAliveAttribute>(inherit: true);
            if (attribute == null)
                return TimeSpan.Zero;

            return TimeSpan.FromHours(attribute.Hours)
                    .Add(TimeSpan.FromMinutes(attribute.Minutes));
        }

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