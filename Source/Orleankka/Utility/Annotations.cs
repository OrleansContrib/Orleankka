using System;
using System.Linq;

namespace Orleankka.Utility
{
    namespace Annotations
    {
        /// <summary>
        ///   Indicates that the function argument should be string literal and match one  of the parameters of the caller
        ///   function.
        ///   For example, <see cref="ArgumentNullException" /> has such parameter.
        /// </summary>
        [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
        sealed class InvokerParameterNameAttribute : Attribute
        {}

        /// <summary>
        ///   Indicates that the marked method is assertion method, i.e. it halts control flow if one of the conditions is
        ///   satisfied.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
        sealed class AssertionMethodAttribute : Attribute
        {}
    }
}
