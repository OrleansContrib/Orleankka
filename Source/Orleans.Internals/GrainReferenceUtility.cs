using Orleans.Runtime;

namespace Orleans.Internals
{
    class GrainReferenceUtility
    {
        public static GrainReference FromKeyString(string key) => GrainReference.FromKeyString(key, null);
    }
}