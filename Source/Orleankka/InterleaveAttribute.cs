using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class InterleaveAttribute : Attribute
    {
        internal readonly Type Message;

        public InterleaveAttribute(Type message)
        {
            Requires.NotNull(message, "message"); 
            Message = message;
        }
    }
}
