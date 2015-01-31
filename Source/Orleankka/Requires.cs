using System;
using System.Linq;

namespace Orleankka
{
    using Annotations;

    static class Requires
    {
        [AssertionMethod]
        public static void NotNull<T>(T argument, [InvokerParameterName] string argumentName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        [AssertionMethod]
        public static void NotNullOrEmpty(string argument, [InvokerParameterName] string argumentName)
        {
            if (string.IsNullOrEmpty(argument))
                throw new ArgumentNullException(argument, argumentName);
        }

        [AssertionMethod]
        public static void NotNullOrWhitespace(string argument, [InvokerParameterName] string argumentName)
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException(argumentName + " cannot consist of whitespace chars only", argumentName);
        }
    }
}