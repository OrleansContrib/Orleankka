using System.Linq;

using Orleans;

namespace Orleankka.Cluster
{
    using Core;

    static class DashboardIntegration
    {
        public static string Format(IIncomingGrainCallContext ctx)
        {
            var methodName = ctx.InterfaceMethod.Name;
            if (!(ctx.Grain is IActorEndpoint) ||
                methodName != nameof(IActorEndpoint.Receive) && 
                methodName != nameof(IActorEndpoint.ReceiveVoid) && 
                methodName != nameof(IActorEndpoint.Notify))
                return methodName;

            var argumentType = ctx.Arguments[0]?.GetType();
            if (argumentType == null)
                return $"{ctx.InterfaceMethod.Name}(NULL)";

            return argumentType.IsGenericType
                ? $"{argumentType.Name}<{string.Join(",", argumentType.GenericTypeArguments.Select(x => x.Name))}>"
                : argumentType.Name;
        }
    }
}