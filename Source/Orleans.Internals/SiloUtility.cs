using System;
using System.Reflection;

using Orleans.Runtime;

namespace Orleans.Internals
{
    static class SiloUtility
    {
        public static IServiceProvider GetServiceProvider(this Silo silo)
        {
            var servicesProperty = typeof(Silo).GetProperty("Services", BindingFlags.Instance | BindingFlags.NonPublic);
            if (servicesProperty == null)
                throw new InvalidOperationException("Hey, who moved my cheese? Silo doesn't have internal 'Services' property anymore!");

            return (IServiceProvider)servicesProperty.GetValue(silo);
        }
    }
}