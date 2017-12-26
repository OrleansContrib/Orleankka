using System.Reflection;

using Orleans;
using Orleans.CodeGeneration;

namespace Orleankka.Cluster
{
    using Core;

    static class DashboardIntegration
    {
        public static string Format(MethodInfo method, InvokeMethodRequest request, IGrain grain)
        {
            if (method == null)
                return "Unknown";

            if (!(grain is IActorGrain))
                return method.Name;

            if (method.Name != nameof(IActorGrain.Receive) && 
                method.Name != nameof(IActorGrain.ReceiveVoid) && 
                method.Name != nameof(IActorGrain.Notify))
                return method.Name;

            return request.Arguments[0]?.GetType().Name ?? $"{method.Name}(NULL)";
        }
    }
}