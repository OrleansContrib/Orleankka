using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleankka
{
    class Message
    {
        static readonly Dictionary<Type, bool> interleaved = 
                    new Dictionary<Type, bool>();

        internal static void Register(Type type)
        {
            var attribute = type.GetCustomAttribute<MessageAttribute>(inherit: true);
            
            if (attribute == null)
                return;

            interleaved.Add(type, attribute.Interleave);
        }

        internal static void Reset()
        {
            interleaved.Clear();
        }

        internal static bool Interleaved(Type type)
        {
            return interleaved.Find(type, false);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MessageAttribute : Attribute
    {
        public bool Interleave { get; set; }
        public bool Mutable    { get; set; }
    }
}
