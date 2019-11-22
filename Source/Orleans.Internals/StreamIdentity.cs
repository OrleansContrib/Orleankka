using Orleans.Streams;

namespace Orleans.Internals
{
    /// <summary>
    /// FOR INTERNAL USE ONLY!
    /// </summary>
    public struct StreamIdentity
    {
        public readonly string Provider;
        public readonly string Id;

        internal StreamIdentity(StreamId stream)
        {
            Provider = stream.ProviderName;
            Id = stream.Namespace;
        }
    }
}