using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orleankka
{
    using Core.Streams;
    using Utility;

    public class StreamSubscriptionSpecification
    {
        internal string Type;
        readonly Func<string, string> matcher;
        readonly Func<object, string> selector;
        readonly Func<object, bool> filter;
        public readonly string Provider;

        public StreamSubscriptionSpecification(string provider, Func<string, string> matcher, Func<object, string> selector = null, Func<object, bool> filter = null)
        {
            Requires.NotNullOrWhitespace(provider, nameof(provider));
            Requires.NotNull(matcher, nameof(matcher));

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

        public static StreamSubscriptionSpecification MatchExact(string provider, string source, string target, Func<object, string> selector = null, Func<object, bool> filter = null)
        {
            Func<string, string> matcher = stream => stream == source ? target: null;
            return new StreamSubscriptionSpecification(provider, matcher, selector, filter);
        }

        public static StreamSubscriptionSpecification MatchPattern(string provider, string source, string target, Func<object, string> selector = null, Func<object, bool> filter = null)
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

            return new StreamSubscriptionSpecification(provider, matcher, selector, filter);
        }
    }
}