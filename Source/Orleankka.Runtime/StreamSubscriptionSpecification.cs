using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    using Core;
    using Core.Streams;
    using Utility;

    public class StreamSubscriptionSpecification
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

            var filter = BuildFilter(attribute.Filter, actor, dispatcher);
            var selector = BuildTargetSelector(attribute.Target, actor);

            var provider = parts[0];
            var source = parts[1];

            var isRegex = source.StartsWith("/") &&
                          source.EndsWith("/");

            if (!isRegex)
                return MatchExact(actor, provider, source, attribute.Target, selector, filter);

            var pattern = source.Substring(1, source.Length - 2);
            return MatchPattern(actor, provider, pattern, attribute.Target, selector, filter);
        }

        static Exception InvalidSpecification(Type actor, string error)
        {
            string message = $"StreamSubscription attribute defined on '{actor}' {error}";
            return new InvalidOperationException(message);
        }

        static Func<object, bool> BuildFilter(string filter, Type actor, Dispatcher dispatcher)
        {
            if (filter == null)
                return item => dispatcher.CanHandle(item.GetType());

            if (filter == "*")
                return item => true;

            if (!filter.EndsWith("()"))
                throw new InvalidOperationException("Filter string value is missing '()' function designator");

            var method = GetStaticMethod(filter, actor);
            if (method == null)
                throw new InvalidOperationException("Filter function should be a static method");

            return (Func<object, bool>)method.CreateDelegate(typeof(Func<object, bool>));
        }

        static Func<object, string> BuildTargetSelector(string target, Type actor)
        {
            if (!target.EndsWith("()"))
                return null;

            var method = GetStaticMethod(target, actor);
            if (method == null)
                throw new InvalidOperationException("Target function should be a static method");

            return (Func<object, string>)method.CreateDelegate(typeof(Func<object, string>));
        }

        static MethodInfo GetStaticMethod(string methodString, Type type)
        {
            var methodName = methodString.Remove(methodString.Length - 2, 2);
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        internal string Type;
        readonly Func<string, string> matcher;
        readonly Func<object, string> selector;
        readonly Func<object, bool> filter;
        public readonly string Provider;

        StreamSubscriptionSpecification(Type actor, string provider, Func<string, string> matcher, Func<object, string> selector = null, Func<object, bool> filter = null)
        {
            Requires.NotNullOrWhitespace(provider, nameof(provider));
            Requires.NotNull(matcher, nameof(matcher));

            Type = ActorTypeName.Of(actor);
            Provider = provider;
            this.matcher = matcher;
            this.selector = selector;

            this.filter = filter ?? (x => true);
        }

        internal StreamSubscriptionMatch Match(IActorSystem system, string stream)
        {
            var target = matcher(stream);
            if (target == null)
                return StreamSubscriptionMatch.None;

            var receiver = selector == null
                ? (Func<object, Task>) Reference(system, target).Tell
                : (x => Reference(system, selector(x)).Tell(x));

            return new StreamSubscriptionMatch(target, receiver, filter);
        }

        ActorRef Reference(IActorSystem system, string id) => system.ActorOf(new ActorPath(Type, id));

        static StreamSubscriptionSpecification MatchExact(Type actor, string provider, string source, string target, Func<object, string> selector = null, Func<object, bool> filter = null)
        {
            Func<string, string> matcher = stream => stream == source ? target: null;
            return new StreamSubscriptionSpecification(actor, provider, matcher, selector, filter);
        }

        static StreamSubscriptionSpecification MatchPattern(Type actor, string provider, string source, string target, Func<object, string> selector = null, Func<object, bool> filter = null)
        { 
            var pattern = new Regex(source, RegexOptions.Compiled);
            var generator = new Regex(@"(?<placeholder>\{[^\}]+\})", RegexOptions.Compiled);

            Func<string, string> matcher = stream =>
            {
                var match = pattern.Match(stream);

                if (!match.Success)
                    return null;

                return generator.Replace(target, m =>
                {
                    var placeholder1 = m.Value.Substring(1, m.Value.Length - 2);
                    return match.Groups[placeholder1].Value;
                });
            };

            return new StreamSubscriptionSpecification(actor, provider, matcher, selector, filter);
        }
    }
}