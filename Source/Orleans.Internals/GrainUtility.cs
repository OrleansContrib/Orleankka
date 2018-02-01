using Orleans.Runtime;

namespace Orleans.Internals
{
    public static class GrainUtility
    {
        public static IGrainRuntime Runtime(this Grain grain) => grain.Runtime;
    }
}