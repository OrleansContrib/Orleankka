using Orleans.Runtime;

namespace Orleankka.Behaviors
{
    public struct RequestOrigin
    {
        const string Key = "#ORLKKA_BHV_RO";

        internal static void Store(string behavior) => RequestContext.Set(Key, behavior);

        internal static RequestOrigin Restore()
        {
            var behavior = RequestContext.Get(Key);
            if (behavior == null)
                return Null;

            RequestContext.Remove(Key);
            return new RequestOrigin((string)behavior, true);
        }
            
        internal static readonly RequestOrigin Null = new RequestOrigin();

        internal RequestOrigin(string behavior, bool background = false)
        {
            Behavior = behavior;
            IsBackground = background;
        }

        public string Behavior { get; }

        public bool IsBackground { get; }
        public bool IsForeground => !IsBackground;

        public bool IsExternal => Behavior == null;
    }
}