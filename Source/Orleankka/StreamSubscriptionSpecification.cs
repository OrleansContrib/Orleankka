using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Orleankka
{
    using Utility;

    public abstract class StreamSubscriptionSpecification
    {
        public readonly string Provider;

        protected StreamSubscriptionSpecification(string provider)
        {
            Requires.NotNullOrWhitespace(provider, nameof(provider));
            Provider = provider;
        }

        public abstract StreamSubscriptionMatch Match(IActorSystem system, string stream);

        public class MatchExact : StreamSubscriptionSpecification
        {
            readonly string source;
            readonly string target;
            readonly Func<IActorSystem, string, Func<object, Task>> receiver;
            readonly Func<object, bool> filter;

            public MatchExact(string provider, string source, string target, Func<IActorSystem, string, Func<object, Task>> receiver, Func<object, bool> filter = null)
                : base(provider)
            {
                this.source = source;
                this.target = target;
                this.receiver = receiver;
                this.filter = filter ?? (x => true);
            }

            public override StreamSubscriptionMatch Match(IActorSystem system, string stream)
            {
                return stream == source
                        ? new StreamSubscriptionMatch(target, x => receiver(system, target)(x), filter)
                        : StreamSubscriptionMatch.None;
            }
        }

        public class MatchPattern : StreamSubscriptionSpecification
        {
            readonly string target;
            readonly Func<IActorSystem, string, Func<object, Task>> receiver;
            readonly Func<object, bool> filter;
            readonly Regex matcher;
            readonly Regex generator;

            public MatchPattern(string provider, string source, string target, Func<IActorSystem, string, Func<object, Task>> receiver, Func<object, bool> filter = null)
                : base(provider)
            {
                this.target = target;
                this.receiver = receiver;
                this.filter = filter;
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

                return new StreamSubscriptionMatch(id, x => receiver(system, id)(x), filter);
            }
        }
    }
}