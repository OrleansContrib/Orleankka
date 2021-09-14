using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Orleans.Streams;

namespace Orleankka
{
    using System.Linq;

    using Utility;

    [Serializable]
    public class StreamFilter : IEquatable<StreamFilter>
    {
        public static readonly StreamFilter ReceiveAll = new StreamFilter(ReceiveAllCallback);
        static bool ReceiveAllCallback(object item) => true;

        readonly string className;
        readonly string methodName;
        readonly HashSet<Type> items;

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

        public StreamFilter(params Type[] items) 
            : this((IEnumerable<Type>)items)
        {}

        StreamFilter(string className, string methodName, List<Type> items)
        {
            this.className = className;
            this.methodName = methodName;
            this.items = items != null ? new HashSet<Type>(items) : null;
        }

        public StreamFilter(IEnumerable<Type> items)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Requires.NotNull(items, nameof(items));

            // ReSharper disable once PossibleMultipleEnumeration
            this.items = new HashSet<Type>(items);

            if (this.items.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(items),
                    "accepted 'items' list is empty");

            filter = ItemFilter;
        }

        Func<object, bool> Filter()
        {
            if (filter != null)
                return filter;

            if (methodName != null)
                filter = CallbackMethodFilter(className, methodName);

            if (items != null)
                filter = ItemFilter;

            return filter ?? (filter = ReceiveAllCallback);
        }

        bool ItemFilter(object item) => items.Contains(item.GetType());

        static Func<object, bool> CallbackMethodFilter(string className, string methodName)
        {
            var type = Type.GetType(className);
            Debug.Assert(type != null);

            var method = type.GetMethod(methodName, BindingFlags.Public |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return (Func<object, bool>) method.CreateDelegate(typeof(Func<object, bool>));
        }

        bool ShouldReceive(object item) => Filter()(item);

        internal class Internal
        {
            public static bool Predicate(IStreamIdentity stream, object filterData, object item)
            {
                var filter = (StreamFilter)filterData;
                return filter.ShouldReceive(item);
            }
        }

        public StreamFilterData Serialize()
        {
            return new StreamFilterData 
            {
                ClassName = className,
                MethodName = methodName,
                Items = items?.ToList()
            };
        }

        public static StreamFilter Deserialize(StreamFilterData data)
        {
            return new StreamFilter(data.ClassName, data.MethodName, data.Items);
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

    [Serializable]
    public class StreamFilterData
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public List<Type> Items { get; set; }
    }
}