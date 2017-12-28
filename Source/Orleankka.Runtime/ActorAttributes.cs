using System;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Utility;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]		
    public class StreamSubscriptionAttribute : Attribute		
    {		
        public string Source;		
        public string Target;		
        public string Filter;		
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class KeepAliveAttribute : Attribute
    {
        internal static TimeSpan Timeout(Type actor)
        {
            var attribute = actor.GetCustomAttribute<KeepAliveAttribute>(inherit: true);
            if (attribute == null)
                return TimeSpan.Zero;

            var result = TimeSpan.FromHours(attribute.Hours)
                .Add(TimeSpan.FromMinutes(attribute.Minutes));

            if (result < TimeSpan.FromMinutes(1))
                throw new ArgumentException(
                    "Minimum activation GC timeout is 1 minute. Actor: " + actor);

            return result;
        }

        public double Minutes;
        public double Hours;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AutorunAttribute : Attribute
    {
        internal static string[] From(Type actor)
        {
            return actor.GetCustomAttributes<AutorunAttribute>(inherit: true)
                        .Select(attribute => attribute.Id)
                        .ToArray();
        }

        public readonly string Id;

        public AutorunAttribute(string id)
        {
            Requires.NotNullOrWhitespace(id, nameof(id));
            Id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StickyAttribute : Attribute
    {
        internal static bool IsApplied(Type actor) => 
            actor.GetCustomAttribute<StickyAttribute>(inherit: true) != null;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class InvokerAttribute : Attribute
    {
        internal static string From(Type actor) => 
            actor.GetCustomAttribute<InvokerAttribute>(inherit: true)?.Name;

        public readonly string Name;

        public InvokerAttribute(string name)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class BehaviorAttribute : Attribute
    {
        public bool Background { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TraitAttribute : Attribute
    {}
}