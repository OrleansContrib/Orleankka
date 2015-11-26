using System;
using System.Diagnostics;
using System.Reflection;

using Orleans.Streams;

namespace Orleankka
{
    using Core;
    using Utility;

    [Serializable]
    public class StreamFilter : IEquatable<StreamFilter>
    {
        public static readonly StreamFilter ReceiveAll = new StreamFilter(ReceiveAllCallback);
        static bool ReceiveAllCallback(object item) => true;

        readonly string className;
        readonly string methodName;

        [NonSerialized] Func<object, bool> filter;

        public StreamFilter(Func<object, bool> filter)
        {
            Requires.NotNull(filter, nameof(filter));
            this.filter = filter;

            var method = filter.Method;
            if (!method.IsStatic)
                throw new ArgumentException("Filter must be a static function");

            Debug.Assert(method.DeclaringType != null);
            className  = method.DeclaringType.AssemblyQualifiedName;
            methodName = method.Name;
        }

        internal StreamFilter(Actor actor)
        {
            className = actor.Prototype.Code;
            filter = DeclaredHandlerOnlyFilter(className);
        }

        Func<object, bool> Filter => filter ?? (filter = methodName != null
                                                ? CallbackMethodFilter(className, methodName)
                                                : DeclaredHandlerOnlyFilter(className));

        static Func<object, bool> CallbackMethodFilter(string className, string methodName)
        {
            var type = Type.GetType(className);
            Debug.Assert(type != null);

            var method = type.GetMethod(methodName, BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Static);

            return (Func<object, bool>) method.CreateDelegate(typeof(Func<object, bool>));
        }

        static Func<object, bool> DeclaredHandlerOnlyFilter(string actorCode)
        {
            var actor = ActorPrototype.Of(actorCode);
            return x => actor.DeclaresHandlerFor(x.GetType());
        }

        bool ShouldReceive(object item)
        {
            return Filter(item);
        }

        internal class Internal
        {
            public static bool Predicate(IStreamIdentity stream, object filterData, object item)
            {
                var filter = (StreamFilter)filterData;
                return filter.ShouldReceive(item);
            }
        }

        public bool Equals(StreamFilter other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(methodName, other.methodName) && 
                   string.Equals(className, other.className);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || 
                   obj.GetType() == GetType() && Equals((StreamFilter) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((methodName?.GetHashCode() ?? 0) * 397) ^ (className?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(StreamFilter left, StreamFilter right) => Equals(left, right);
        public static bool operator !=(StreamFilter left, StreamFilter right) => !Equals(left, right);
    }
}