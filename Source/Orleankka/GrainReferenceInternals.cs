using System;
using System.Linq;
using System.Reflection;

using Orleans.Runtime;

namespace Orleankka
{
    partial class ClientEndpoint
    {
        /// <summary>
        /// HACK to get GrainReference from KeyString
        /// </summary>
        static class GrainReferenceInternals
        {
            delegate GrainReference FromKeyStringDelegate(string key, IGrainReferenceRuntime runtime);
            static readonly FromKeyStringDelegate fromKeyString;

            static GrainReferenceInternals()
            {
                var method = typeof(GrainReference).GetMethod("FromKeyString", BindingFlags.Static | BindingFlags.NonPublic);
                fromKeyString = (FromKeyStringDelegate) Delegate.CreateDelegate(typeof(FromKeyStringDelegate), method);
            }

            public static GrainReference FromKeyString(string key) => fromKeyString(key, null);
        }
    }
}