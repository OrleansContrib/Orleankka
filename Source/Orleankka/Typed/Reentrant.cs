using System;
using System.Linq;

namespace Orleankka.Typed
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, 
                    AllowMultiple = false, Inherited = true)]
    public class ReentrantAttribute : Attribute
    {}
}
