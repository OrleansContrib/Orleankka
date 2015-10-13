using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
        readonly Type actor;
        readonly Func<object, bool> filter;

        StreamSubscriptionSpecification(string provider, string source, string target, Type actor, string filter)
        {
            Provider = provider;
            this.source = source;
            this.target = target;
            this.actor  = actor;
            this.filter = Build(filter, actor);
        }

        static Func<object, bool> Build(string filter, Type actor)
        {
            if (filter == null)
                return item => true;

            var methodName = filter;
            var method = actor.GetMethod(methodName);

            return (Func<object, bool>) method.CreateDelegate(typeof(Func<object, bool>));
        }

        public abstract StreamSubscriptionMatch Match(string stream);

        class MatchExact : StreamSubscriptionSpecification
        {
            public MatchExact(string provider, string source, string target, Type actor, string filter)
                : base(provider, source, target, actor, filter)
            {}

            public override StreamSubscriptionMatch Match(string stream)
            {
                return stream == source 
                    ? new StreamSubscriptionMatch(actor, target, filter) 
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

            public override StreamSubscriptionMatch Match(string stream)
            {
                var match = matcher.Match(stream);

                if (!match.Success)
                    return StreamSubscriptionMatch.None;

                var id = generator.Replace(target, m =>
                {
                    var placeholder = m.Value.Substring(1, m.Value.Length - 2);
                    return match.Groups[placeholder].Value;
                });

                return new StreamSubscriptionMatch(actor, id, filter);
            }
        }
    }
}