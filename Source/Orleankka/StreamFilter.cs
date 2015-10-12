using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

using Orleans.Streams;

namespace Orleankka
{
    using Utility;

    [Serializable]
    public class StreamFilter : ISerializable
    {
        string methodName;
        string className;

        [NonSerialized]
        Func<object, bool> filter;

        public StreamFilter(Func<object, bool> filter)
        {
            Requires.NotNull(filter, nameof(filter));
            this.filter = filter;
            Dehydrate();
        }

        protected StreamFilter(SerializationInfo info, StreamingContext context)
        {
            methodName = info.GetString("MethodName");
            className = info.GetString("ClassName");
            filter = Rehydrate(className, methodName);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MethodName", methodName);
            info.AddValue("ClassName", className);
        }

        bool ShouldReceive(object item)
        {
            if (filter == null)
                filter = Rehydrate(className, methodName);

            return filter(item);
        }

        static Func<object, bool> Rehydrate(string className, string methodName)
        {
            var type = Type.GetType(className);

            Debug.Assert(type != null);
            var method = type.GetMethod(methodName);

            return (Func<object, bool>) method.CreateDelegate(typeof(Func<object, bool>));
        }

        void Dehydrate()
        {
            var method = filter.Method;
            if (!CheckIsStatic(method))
                throw new ArgumentException("Filter function must be static and not abstract.");

            Debug.Assert(method.DeclaringType != null);
            className  = method.DeclaringType.AssemblyQualifiedName;
            methodName = method.Name;
        }

        static bool CheckIsStatic(MethodBase method)
        {
            return method.IsStatic && !method.IsAbstract;
        }

        public static bool Predicate(IStreamIdentity stream, object filterData, object item)
        {
            var filter = (StreamFilter)filterData;
            return filter.ShouldReceive(item);
        }
    }
}