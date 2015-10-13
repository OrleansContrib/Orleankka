using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

using Orleans.Streams;

namespace Orleankka
{
    using Utility;

    [Serializable]
    public class StreamFilter : ISerializable, IEquatable<StreamFilter>
    {
        readonly string methodName;
        readonly string className;

        [NonSerialized]
        readonly Func<object, bool> filter;

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

        protected StreamFilter(SerializationInfo info, StreamingContext context)
        {
            methodName = info.GetString("MethodName");
            className = info.GetString("ClassName");

            var type = Type.GetType(className);
            Debug.Assert(type != null);

            var method = type.GetMethod(methodName, BindingFlags.Public | 
                                                    BindingFlags.NonPublic |  
                                                    BindingFlags.Static);

            filter = (Func<object, bool>) method.CreateDelegate(typeof(Func<object, bool>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MethodName", methodName);
            info.AddValue("ClassName", className);
        }

        bool ShouldReceive(object item)
        {
            return filter(item);
        }

        public static bool Predicate(IStreamIdentity stream, object filterData, object item)
        {
            var filter = (StreamFilter)filterData;
            return filter.ShouldReceive(item);
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