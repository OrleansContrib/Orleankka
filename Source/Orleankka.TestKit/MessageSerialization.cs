using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Orleankka.TestKit
{
    using Orleans.Serialization;

    public class MessageSerialization
    {
        internal static readonly MessageSerialization Default = new();
        
        readonly Serializer serializer;
        readonly MethodInfo deserializer;

        public MessageSerialization(IServiceProvider roundtripSerializerProvider = null)
        {
            if (roundtripSerializerProvider == null)
                return;

            serializer = roundtripSerializerProvider.GetRequiredService<Serializer>();
            deserializer = serializer.GetType().GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.Public, new[]{typeof(byte[])});
        }

        public object Roundtrip(object obj) => serializer != null
            ? Deserialize(obj)  
            : obj;

        object Deserialize(object obj)
        {
            var bytes = serializer.SerializeToArray(obj);
            var method = deserializer.MakeGenericMethod(obj.GetType());
            return method.Invoke(serializer, new object[]{bytes});
        }
    }
}