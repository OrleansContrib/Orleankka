using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Orleankka.Utility;

using Orleans.Runtime;

namespace Orleankka.Legacy.Streams
{
    static class StreamSubscriptionSpecificationBuilder
    {
        internal static StreamSubscriptionSpecification Build(Type grainType, StreamSubscriptionAttribute attribute, IDispatcherRegistry registry)
        {
            if (string.IsNullOrWhiteSpace(attribute.Source))
                throw InvalidSpecification(grainType, "has null or whitespace only value of Source");

            if (string.IsNullOrWhiteSpace(attribute.Target))
                throw InvalidSpecification(grainType, "has null or whitespace only value of Target");

            if (attribute.Filter != null && string.IsNullOrWhiteSpace(attribute.Filter))
                throw InvalidSpecification(grainType, "has whitespace only value of Filter");

            var parts = attribute.Source.Split(new[] { ":" }, 2, StringSplitOptions.None);
            if (parts.Length != 2)
                throw InvalidSpecification(grainType, $"has invalid Source specification: {attribute.Source}");

            var provider = parts[0];
            var source = parts[1];

            var matcher = BuildMatcher(null, source, attribute.Target);
            var selector = BuildTargetSelector(attribute.Target, grainType);
            var filter = BuildFilter(attribute.Filter, grainType, registry);

            return new StreamSubscriptionSpecification(grainType, provider, matcher, selector, filter);
        }

        static Exception InvalidSpecification(Type actor, string error)
        {
            var message = $"StreamSubscription attribute defined on '{actor}' {error}";
            return new InvalidOperationException(message);
        }

        static StreamMatchesFunc BuildMatcher(string @namespace, string source, string target)
        {
            var isRegex = source.StartsWith("/") && source.EndsWith("/");
            if (!isRegex)
                return (StreamId stream, out string targetId) =>
                {
                    var ns = stream.GetNamespace();
                    var key = stream.GetKeyAsString();

                    if (ns == @namespace && key == source)
                    {
                        targetId = target;
                        return true;
                    }

                    targetId = null;
                    return false;
                };

            var pattern = new Regex(source.Substring(1, source.Length - 2), RegexOptions.Compiled);
            var generator = new Regex(@"(?<placeholder>\{[^\}]+\})", RegexOptions.Compiled);

            return (StreamId stream, out string targetId) =>
            {
                var ns = stream.GetNamespace();
                var key = stream.GetKeyAsString();

                if (ns == @namespace)
                {
                    var match = pattern.Match(key);
                    if (match.Success)
                    {
                        targetId = generator.Replace(target, m =>
                        {
                            var placeholder = m.Value.Substring(1, m.Value.Length - 2);
                            return match.Groups[placeholder].Value;
                        });

                        return true;
                    }
                }

                targetId = null;
                return false;
            };
        }

        static ShouldSendMessageFunc BuildFilter(string filter, Type actor, IDispatcherRegistry registry)
        {
            if (filter == null)
                return item => registry.GetDispatcher(actor).CanHandle(item.GetType());

            if (filter == "*")
                return _ => true;

            if (!filter.EndsWith("()"))
                throw new InvalidOperationException("Filter string value is missing '()' function designator");

            var method = GetStaticMethod(filter, actor);
            if (method == null)
                throw new InvalidOperationException("Filter function should be a static method");

            return (ShouldSendMessageFunc)method.CreateDelegate(typeof(ShouldSendMessageFunc));
        }

        static SelectTargetFunc BuildTargetSelector(string target, Type actor)
        {
            if (!target.EndsWith("()"))
                return null;

            var method = GetStaticMethod(target, actor);
            if (method == null)
                throw new InvalidOperationException("Target function should be a static method");

            return (SelectTargetFunc)method.CreateDelegate(typeof(SelectTargetFunc));
        }

        static MethodInfo GetStaticMethod(string methodString, Type type)
        {
            var methodName = methodString.Remove(methodString.Length - 2, 2);
            return type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }

    delegate bool StreamMatchesFunc(StreamId streamId, out string target);

    delegate string SelectTargetFunc(object message);

    delegate bool ShouldSendMessageFunc(object message);

    class StreamSubscriptionSpecification
    {
        readonly ShouldSendMessageFunc filterMessageFunc;
        readonly StreamMatchesFunc matchStreamIdMatcher;
        readonly SelectTargetFunc targetSelector;

        public StreamSubscriptionSpecification(
        Type actorType,
        string providerName,
        StreamMatchesFunc matchStreamFunc,
        SelectTargetFunc selectTargetFunc,
        ShouldSendMessageFunc filterMessageFunc)
        {
            Requires.NotNull(actorType, nameof(actorType));
            Requires.NotNullOrWhitespace(providerName, nameof(providerName));
            Requires.NotNull(matchStreamFunc, nameof(matchStreamFunc));

            ActorType = actorType;
            InterfaceType = ActorGrain.InterfaceOf(actorType);
            ProviderName = providerName;

            matchStreamIdMatcher = matchStreamFunc;
            targetSelector = selectTargetFunc;
            this.filterMessageFunc = filterMessageFunc;
        }

        public Type ActorType { get; }
        public Type InterfaceType { get; }
        public string ProviderName { get; }

        public bool IsMatch(StreamId streamId, out StreamSubscriptionMatch match)
        {
            if (matchStreamIdMatcher(streamId, out var target))
            {
                match = new StreamSubscriptionMatch(InterfaceType, target, targetSelector, filterMessageFunc);
                return true;
            }

            match = null;
            return false;
        }
    }

    class StreamSubscriptionMatch
    {
        public StreamSubscriptionMatch(Type interfaceType, string target, SelectTargetFunc actorIdSelector, ShouldSendMessageFunc shouldSendMessage)
        {
            InterfaceType = interfaceType;
            Target = target;
            SelectTarget = actorIdSelector ?? (_ => target);
            ShouldSendMessage = shouldSendMessage;
        }

        public Type InterfaceType { get; }
        public string Target { get; }
        public SelectTargetFunc SelectTarget { get; }
        public ShouldSendMessageFunc ShouldSendMessage { get; }

        public string MessageDelivererId => $"{InterfaceType.FullName}/{Target}";
    }
}