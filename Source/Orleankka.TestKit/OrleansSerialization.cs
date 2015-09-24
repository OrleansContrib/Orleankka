using System;
using System.Reflection;

using Orleans.Serialization;

namespace Orleankka.TestKit
{
    using Core;

    /// <summary>
    /// An instance of BufferPool set on static BufferPool.GlobalPool field 
    /// is required by internal ByteArrayBuilder which is used by ByteTokenStreamWriter
    /// </summary>
    /// <remarks>Don't try this at home, kids!</remarks>
    class OrleansSerialization
    {
        internal static void Hack() {}

        static OrleansSerialization()
        {
            var bufferPoolType = typeof(BinaryTokenStreamWriter).Assembly.GetType("Orleans.Runtime.BufferPool");

            const int defaultBufferPoolBufferSize = 4 * 1024;
            const int defaultBufferPoolMaxSize = 10000;
            const int defaultBufferPoolPreallocationSize = 250;

            var constructorArguments = new object[]
            {
                defaultBufferPoolBufferSize,
                defaultBufferPoolMaxSize,
                defaultBufferPoolPreallocationSize,
                "Global"
            };

            var bufferPool = Activator.CreateInstance(bufferPoolType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, constructorArguments, null);

            var globalPoolField = bufferPoolType.GetField("GlobalPool", BindingFlags.Public | BindingFlags.Static);
            globalPoolField?.SetValue(null, bufferPool);
        }

        internal static object Reserialize(IMessageSerializer serializer, object message)
        {
            if (serializer == null)
                return message;

            var writer = new BinaryTokenStreamWriter();
            serializer.Serialize(message, writer);

            var bytes = writer.ToByteArray();
            var reader = new BinaryTokenStreamReader(bytes);

            return serializer.Deserialize(reader);
        }
    }
}