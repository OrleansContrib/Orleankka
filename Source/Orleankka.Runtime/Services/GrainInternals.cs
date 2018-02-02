using System;
using System.Reflection;

using Orleans;
using Orleans.Runtime;

namespace Orleankka.Services
{
    /// <summary>
    /// HACK to get grain's runtime
    /// </summary>
    public static class GrainInternals
    {
        static readonly Func<Grain, IGrainRuntime> getRuntime;

        static GrainInternals()
        {
            var property = typeof(Grain).GetProperty("Runtime", BindingFlags.Instance | BindingFlags.NonPublic);
            getRuntime = (Func<Grain, IGrainRuntime>) Delegate.CreateDelegate(typeof(Func<Grain, IGrainRuntime>), property.GetMethod);
        }

        public static IGrainRuntime Runtime(this Grain grain) => getRuntime(grain);    
    }
}