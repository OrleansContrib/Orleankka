namespace Orleans.Internals
{
    using Concurrency;

    static class AttributeUtility
    {
        public static string CallbackMethodName(this MayInterleaveAttribute att) => att.CallbackMethodName;
    }
}