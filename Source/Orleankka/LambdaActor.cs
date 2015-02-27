using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleankka
{
    using Utility;

    public abstract class LambdaActor : Actor
    {
        protected void On<TRequest, TResult>(Func<TRequest, TResult> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest, TResult>(Func<TRequest, Task<TResult>> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Action<TRequest> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }

        protected void On<TRequest>(Func<TRequest, Task> handler)
        {
            Requires.NotNull(handler, "handler");
            Prototype.RegisterHandler(handler.Method);
        }
    }
}
