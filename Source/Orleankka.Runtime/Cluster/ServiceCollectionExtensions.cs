using System;
using System.Linq;

namespace Orleankka.Cluster
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    static class ServiceCollectionExtensions
    {
        public static void Decorate<T>(this IServiceCollection services, Func<T, T> decorator) where T : class
        {
            var registered = services.First(s => s.ServiceType == typeof(T));
            
            var factory = registered.ImplementationFactory;
            if (factory == null && registered.ImplementationType != null)
                services.TryAddSingleton(registered.ImplementationType);

            services.Replace(new ServiceDescriptor(typeof(T), sp =>
            {
                var inner = registered.ImplementationInstance;
                if (inner != null)
                    return decorator((T) inner);

                inner = factory == null 
                    ? sp.GetService(registered.ImplementationType) 
                    : factory(sp);

                return decorator((T) inner);
                
            }, 
            registered.Lifetime));
        }
    }
}