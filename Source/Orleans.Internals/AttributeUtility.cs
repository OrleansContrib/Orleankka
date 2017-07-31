using System;
using System.Linq;

namespace Orleans.Internals
{
    using Concurrency;

    internal static class AttributeUtility
    {
        public static string CallbackMethodName(this MayInterleaveAttribute att) => att.CallbackMethodName;
    }
}