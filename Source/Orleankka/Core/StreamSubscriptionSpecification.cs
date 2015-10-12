using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Orleans.Streams;

namespace Orleankka.Core
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

            var parts = attribute.Source.Split(new[] {":"}, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw InvalidSpecification(actor, $"has invalid Source specification: {attribute.Source}");

            var provider = parts[0];
            var source   = parts[1];
            var target   = attribute.Target;

            var isRegex  = source.StartsWith("/") && 
                           source.EndsWith("/");
            if (!isRegex)
                return new MatchExact(provider, source, target, actor);

            var pattern = source.Substring(1, source.Length - 2);
            return new MatchPattern(provider, pattern, target, actor);
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

        StreamSubscriptionSpecification(string provider, string source, string target, Type actor)
        {
            Provider = provider;
            this.source = source;
            this.target = target;
            this.actor  = actor;
        }

        public abstract StreamSubscriptionMatch Match(string stream);

        class MatchExact : StreamSubscriptionSpecification
        {
            public MatchExact(string provider, string source, string target, Type actor)
                : base(provider, source, target, actor)
            {}

            public override StreamSubscriptionMatch Match(string stream)
            {
                return stream == source 
                    ? new StreamSubscriptionMatch(actor, target) 
                    : StreamSubscriptionMatch.None;
            }
        }

        class MatchPattern : StreamSubscriptionSpecification
        {
            readonly Regex matcher;
            readonly Regex generator;

            public MatchPattern(string provider, string source, string target, Type actor)
                : base(provider, source, target, actor)
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

                return new StreamSubscriptionMatch(actor, id);
            }
        }
    }

    struct StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        public readonly Type ActorType;
        public readonly string ActorId;

        public StreamSubscriptionMatch(Type actorType, string actorId)
        {
            ActorId = actorId;
            ActorType = actorType;
        }
    }
}