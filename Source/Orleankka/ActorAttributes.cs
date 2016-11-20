using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Orleankka.Utility;

namespace Orleankka
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
    public class ActorTypeAttribute : Attribute
    {
        internal readonly string Name;

        public ActorTypeAttribute(string name)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            if (name.Contains(ActorPath.Separator[0]))
                throw new ArgumentException($"Actor type name cannot contain path separator: {name}");

            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ReentrantAttribute : Attribute
    {
        internal static Func<object, bool> Predicate(Type actor, out bool reentrant)
        {
            reentrant = false;

            var attributes = actor.GetCustomAttributes<ReentrantAttribute>(inherit: true).ToArray();
            if (attributes.Length == 0)
                return null;

            var fullyReentrant = attributes.Where(x => x.message == null && x.callback == null).ToArray();
            var selectedMessageType = attributes.Where(x => x.message != null).ToArray();
            var determinedByCallbackMethod = attributes.Where(x => x.callback != null).ToArray();

            if (fullyReentrant.Any() && (selectedMessageType.Any() || determinedByCallbackMethod.Any()))
                throw new InvalidOperationException(
                    $"'{actor}' actor can be only designated either as fully reentrant " +
                    "or partially reentrant. Choose one of the approaches");

            if (fullyReentrant.Length > 1)
                throw new InvalidOperationException(
                    $"'{actor}' actor can't have multiple [Reentrant] attributes specified");

            if (fullyReentrant.Any())
            {
                reentrant = true;
                return null;
            }

            if (selectedMessageType.Any() && determinedByCallbackMethod.Any())
                throw new InvalidOperationException(
                    $"'{actor}' actor can be designated as partially reentrant either by specifying callback method name " +
                    "or by specifying selected message types. Choose one of the approaches");

            if (determinedByCallbackMethod.Length > 1)
                throw new InvalidOperationException(
                    $"'{actor}' actor can't have multiple [Reentrant(\"callback\")] attributes specified");

            if (determinedByCallbackMethod.Any())
                return DeterminedByCallbackMethod(actor, determinedByCallbackMethod[0].callback);
                    
            return MesageTypeBased(actor, selectedMessageType);
        }

        static Func<object, bool> DeterminedByCallbackMethod(Type actor, string callbackMethod)
        {
            var method = actor.GetMethod(callbackMethod, BindingFlags.Public | BindingFlags.Static);
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
                    $"Expected: public static bool {callbackMethod}(object msg)");

            var parameter = Expression.Parameter(typeof(object));
            var call = Expression.Call(null, method, parameter);
            var predicate = Expression.Lambda<Func<object, bool>>(call, parameter).Compile();

            return predicate;
        }

        static Func<object, bool> MesageTypeBased(Type actor, ReentrantAttribute[] attributes)
        {
            var messages = new HashSet<Type>();

            foreach (var attribute in attributes)
            {
                if (messages.Contains(attribute.message))
                    throw new InvalidOperationException(
                        $"{attribute.message} was already registered as Reentrant for {actor}");

                messages.Add(attribute.message);
            }

            return message => messages.Contains(message.GetType());
        }

        readonly string callback;
        readonly Type message;

        public ReentrantAttribute()
        {}

        public ReentrantAttribute(Type message)
        {
            Requires.NotNull(message, nameof(message));
            this.message = message;
        }

        public ReentrantAttribute(string callback)
        {
            Requires.NotNullOrWhitespace(callback, nameof(callback));
            this.callback = callback;
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
}