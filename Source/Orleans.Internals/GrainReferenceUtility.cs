using Orleans.Runtime;

namespace Orleans.Internals
{
    public static class GrainReferenceUtility
    {
        public static GrainReference FromKeyString(string key) => GrainReference.FromKeyString(key, null);
    }
}