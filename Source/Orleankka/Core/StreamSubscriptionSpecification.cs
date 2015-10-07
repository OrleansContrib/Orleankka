using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Orleankka.Core
{
    class StreamSubscriptionSpecification
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
            
            return new StreamSubscriptionSpecification(provider, source, target, actor);
        }

        static Exception InvalidSpecification(Type actor, string error)
        {
            string message = $"StreamSubscription attribute defined on '{actor}' {error}";
            return new InvalidOperationException(message);
        }

        public readonly string Provider;
        readonly string target;
        readonly Type actor;

        readonly Regex matcher;
        readonly Regex generator;

        StreamSubscriptionSpecification(string provider, string source, string target, Type actor)
        {
            Provider = provider;

            this.target = target;
            this.actor  = actor;

            matcher   = new Regex(source, RegexOptions.Compiled);
            generator = new Regex(@"(?<placeholder>\{[^\}]+\})", RegexOptions.Compiled);
        }

        public StreamSubscriptionMatch Match(string stream)
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

    struct StreamSubscriptionMatch
    {
        public static readonly StreamSubscriptionMatch None = new StreamSubscriptionMatch();

        public readonly Type Actor;
        public readonly string Id;

        public StreamSubscriptionMatch(Type actor, string id)
        {
            Id = id;
            Actor = actor;
        }
    }
}