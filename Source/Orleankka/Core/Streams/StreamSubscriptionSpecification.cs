using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orleankka.Core.Streams
{
    abstract class StreamSubscriptionSpecification
    {
        internal static IEnumerable<StreamSubscriptionSpecification> From(ActorType type)
        {
            return type.Implementation
                       .GetCustomAttributes<StreamSubscriptionAttribute>(inherit: true)
                       .Select(a => From(type.Implementation, a));
        }

        internal static StreamSubscriptionSpecification From(Type actor, StreamSubscriptionAttribute attribute)
        {
            if (string.IsNullOrWhiteSpace(attribute.Source))
                throw InvalidSpecification(actor, "has null or whitespace only value of Source");

            if (string.IsNullOrWhiteSpace(attribute.Target))
                throw InvalidSpecification(actor, "has null or whitespace only value of Target");

            if (attribute.Filter != null && string.IsNullOrWhiteSpace(attribute.Filter))
                throw InvalidSpecification(actor, "has whitespace only value of Filter");

            var parts = attribute.Source.Split(new[] {":"}, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw InvalidSpecification(actor, $"has invalid Source specification: {attribute.Source}");

            var provider = parts[0];
            var source   = parts[1];
            var target   = attribute.Target;
            var filter   = attribute.Filter;

            var isRegex  = source.StartsWith("/") && 
                           source.EndsWith("/");
            if (!isRegex)
                return new MatchExact(provider, source, target, actor, filter);

            var pattern = source.Substring(1, source.Length - 2);
            return new MatchPattern(provider, pattern, target, actor, filter);
        }

        static Exception InvalidSpecification(Type actor, string error)
        {
            string message = $"StreamSubscription attribute defined on '{actor}' {error}";
            return new InvalidOperationException(message);
        }

        public readonly string Provider;
        readonly string source;
        readonly string target;

        readonly Func<object, bool> filter;
        readonly Func<IActorSystem, string, Func<object, Task>> receiver;

        StreamSubscriptionSpecification(string provider, string source, string target, Type actor, string filter)
        {
            Provider    = provider;
            this.source = source;
            this.target = target;

            this.filter = BuildFilter(filter, actor);
            receiver    = BuildReceiver(target, actor);
        }

        static Func<object, bool> BuildFilter(string filter, Type actor)
        {
            if (filter == null)
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
            if (!target.EndsWith("()"))
            {
                return (system, id) =>
                {
                    var receiver = system.ActorOf(actor, id);
                    return receiver.Tell;
                };
            }

            var method = GetStaticMethod(target, actor);
            if (method == null)
                throw new InvalidOperationException("Target function should be a static method");

            var selector = (Func<object, string>)method.CreateDelegate(typeof(Func<object, string>));
            return (system, id) => (item => system.ActorOf(actor, selector(item)).Tell(item));
        }

        static MethodInfo GetStaticMethod(string methodString, Type type)
        {
            var methodName = methodString.Remove(methodString.Length - 2, 2);
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public abstract StreamSubscriptionMatch Match(IActorSystem system, string stream);

        class MatchExact : StreamSubscriptionSpecification
        {
            public MatchExact(string provider, string source, string target, Type actor, string filter)
                : base(provider, source, target, actor, filter)
            {}

            public override StreamSubscriptionMatch Match(IActorSystem system, string stream)
            {
                return stream == source 
                        ? new StreamSubscriptionMatch(x => receiver(system, target)(x), filter) 
                        : StreamSubscriptionMatch.None;
            }
        }

        class MatchPattern : StreamSubscriptionSpecification
        {
            readonly Regex matcher;
            readonly Regex generator;

            public MatchPattern(string provider, string source, string target, Type actor, string filter)
                : base(provider, source, target, actor, filter)
            {
                matcher = new Regex(source, RegexOptions.Compiled);
                generator = new Regex(@"(?<placeholder>\{[^\}]+\})", RegexOptions.Compiled);
            }

            public override StreamSubscriptionMatch Match(IActorSystem system, string stream)
            {
                var match = matcher.Match(stream);

                if (!match.Success)
                    return StreamSubscriptionMatch.None;

                var id = generator.Replace(target, m =>
                {
                    var placeholder = m.Value.Substring(1, m.Value.Length - 2);
                    return match.Groups[placeholder].Value;
                });

                return new StreamSubscriptionMatch(x => receiver(system, id)(x), filter);
            }
        }
    }
}