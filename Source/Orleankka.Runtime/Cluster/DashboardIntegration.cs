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

            if (!(grain is IActorEndpoint))
                return method.Name;

            if (method.Name != nameof(IActorEndpoint.Receive) && 
                method.Name != nameof(IActorEndpoint.ReceiveVoid) && 
                method.Name != nameof(IActorEndpoint.Notify))
                return method.Name;

            return request.Arguments[0]?.GetType().Name ?? $"{method.Name}(NULL)";
        }
    }
}