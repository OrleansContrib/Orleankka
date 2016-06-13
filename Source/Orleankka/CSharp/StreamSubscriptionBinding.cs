using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.CSharp
{
    class StreamSubscriptionBinding
    {
        internal static IEnumerable<StreamSubscriptionSpecification> From(Type actor, Dispatcher dispatcher)
        {
            return actor.GetCustomAttributes<StreamSubscriptionAttribute>(inherit: true)
                        .Select(attribute => From(actor, attribute, dispatcher));
        }

        internal static StreamSubscriptionSpecification From(Type actor, StreamSubscriptionAttribute attribute, Dispatcher dispatcher)
        {
            if (string.IsNullOrWhiteSpace(attribute.Source))
                throw InvalidSpecification(actor, "has null or whitespace only value of Source");

            if (string.IsNullOrWhiteSpace(attribute.Target))
                throw InvalidSpecification(actor, "has null or whitespace only value of Target");

            if (attribute.Filter != null && string.IsNullOrWhiteSpace(attribute.Filter))
                throw InvalidSpecification(actor, "has whitespace only value of Filter");

            var parts = attribute.Source.Split(new[] { ":" }, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw InvalidSpecification(actor, $"has invalid Source specification: {attribute.Source}");

            var provider = parts[0];
            var source = parts[1];

            var isRegex = source.StartsWith("/") &&
                          source.EndsWith("/");

            var filter = BuildFilter(attribute.Filter, actor, dispatcher);
            var receiver = BuildReceiver(attribute.Target, actor);

            if (!isRegex)
                return new StreamSubscriptionSpecification.MatchExact(provider, source, attribute.Target, receiver, filter);

            var pattern = source.Substring(1, source.Length - 2);
            return new StreamSubscriptionSpecification.MatchPattern(provider, pattern, attribute.Target, receiver, filter);
        }

        static Exception InvalidSpecification(Type actor, string error)
        {
            string message = $"StreamSubscription attribute defined on '{actor}' {error}";
            return new InvalidOperationException(message);
        }

        static Func<object, bool> BuildFilter(string filter, Type actor, Dispatcher dispatcher)
        {
            if (filter == null)
                return item => dispatcher.HasRegisteredHandler(item.GetType());

            if (filter == "*")
                return item => true;

            if (!filter.EndsWith("()"))
                throw new InvalidOperationException("Filter string value is missing '()' function designator");

            var method = GetStaticMethod(filter, actor);
            if (method == null)
                throw new InvalidOperationException("Filter function should be a static method");

            return (Func<object, bool>)method.CreateDelegate(typeof(Func<object, bool>));
        }

        static Func<IActorSystem, string, Func<object, Task>> BuildReceiver(string target, Type actor)
        {
            var code = ActorTypeCode.Of(actor);

            if (!target.EndsWith("()"))
            {
                return (system, id) =>
                {
                    var receiver = system.ActorOf(new ActorPath(code, id));
                    return receiver.Tell;
                };
            }

            var method = GetStaticMethod(target, actor);
            if (method == null)
                throw new InvalidOperationException("Target function should be a static method");

            var selector = (Func<object, string>)method.CreateDelegate(typeof(Func<object, string>));
            return (system, _) => (item => system.ActorOf(new ActorPath(code, selector(item))).Tell(item));
        }

        static MethodInfo GetStaticMethod(string methodString, Type type)
        {
            var methodName = methodString.Remove(methodString.Length - 2, 2);
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}