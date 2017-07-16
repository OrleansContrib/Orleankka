using System;
using System.Reflection;

using Orleans.Runtime;
using Orleans.Runtime.Host;

namespace Orleans.Internals
{
    static class SiloUtility
    {
        public static Silo GetSilo(this SiloHost host)
        {
            var siloField = typeof(SiloHost).GetField("orleans", BindingFlags.Instance | BindingFlags.NonPublic);
            if (siloField == null)
                throw new InvalidOperationException("Hey, who moved my cheese? SiloHost doesn't have private 'orleans' field anymore!");

            return (Silo)siloField.GetValue(host);
        }

        public static IServiceProvider GetServiceProvider(this Silo silo)
        {
            var servicesProperty = typeof(Silo).GetProperty("Services", BindingFlags.Instance | BindingFlags.NonPublic);
            if (servicesProperty == null)
                throw new InvalidOperationException("Hey, who moved my cheese? Silo doesn't have internal 'Services' property anymore!");

            return (IServiceProvider)servicesProperty.GetValue(silo);
        }
    }
}