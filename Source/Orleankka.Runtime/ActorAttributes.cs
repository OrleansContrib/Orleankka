using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Orleans.Internals;
using Orleans.Concurrency;

namespace Orleankka
{
    using Utility;
    
    class Interleaving
    {
        internal static Func<object, bool> MayInterleavePredicate(Type actor)
        {
            bool reentrant;
            return MayInterleavePredicate(actor, out reentrant);
        }

        internal static bool IsReentrant(Type actor)
        {
            bool reentrant;
            MayInterleavePredicate(actor, out reentrant);
            return reentrant;
        }

        static Func<object, bool> MayInterleavePredicate(Type actor, out bool reentrant)
        {
            reentrant = false;

            var attributes = actor.GetCustomAttributes(inherit: true).ToArray();
            if (attributes.Length == 0)
                return null;

            var fullyReentrant = attributes.OfType<ReentrantAttribute>().SingleOrDefault();
            var determinedByCallbackMethod = attributes.OfType<MayInterleaveAttribute>().SingleOrDefault();

            if (fullyReentrant != null && determinedByCallbackMethod != null)
                throw new InvalidOperationException(
                    $"'{actor}' actor can be only designated either as fully reentrant " +
                    "or partially reentrant. Choose one of the approaches");

            if (fullyReentrant != null)
            {
                reentrant = true;
                return null;
            }

            return determinedByCallbackMethod != null
                ? DeterminedByCallbackMethod(actor, determinedByCallbackMethod.CallbackMethodName())
                : null;
        }

        static Func<object, bool> DeterminedByCallbackMethod(Type actor, string callbackMethod)
        {
            var method = actor.GetMethod(callbackMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (method == null)
                throw new InvalidOperationException(
                    $"Actor {actor.FullName} doesn't declare public static method " +
                    $"with name {callbackMethod} specified in Reentrant[] attribute");

            if (method.ReturnType != typeof(bool) ||
                method.GetParameters().Length != 1 ||
                method.GetParameters()[0].ParameterType != typeof(object))
                throw new InvalidOperationException(
                    $"Wrong signature of callback method {callbackMethod} " +
                    $"specified in Reentrant[] attribute for actor class {actor.FullName}. \n" +
                    $"Expected: [public] static bool {callbackMethod}(object msg)");

            var parameter = Expression.Parameter(typeof(object));
            var call = Expression.Call(null, method, parameter);
            var predicate = Expression.Lambda<Func<object, bool>>(call, parameter).Compile();

            return predicate;
        }
    }

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