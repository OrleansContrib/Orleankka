using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka.Typed
{
    public sealed class TypedActorRef<TActor> where TActor : TypedActor
    {
        readonly ActorRef @ref;

        internal TypedActorRef(ActorRef @ref)
        {
            this.@ref = @ref;
        }

        public Task Call(Expression<Action<TActor>> expr)
        {
            Requires.NotNull(expr, "expr");
            return CallVoid(expr.Body);
        }

        public Task<TResult> Call<TResult>(Expression<Func<TActor, TResult>> expr)
        {
            Requires.NotNull(expr, "expr");
            return CallResult<TResult>(expr.Body);
        }

        public Task Call(Expression<Func<TActor, Task>> expr)
        {
            Requires.NotNull(expr, "expr");
            return CallVoid(expr.Body);
        }

        public Task<TResult> Call<TResult>(Expression<Func<TActor, Task<TResult>>> expr)
        {
            Requires.NotNull(expr, "expr");
            return CallResult<TResult>(expr.Body);
        }

        Task CallVoid(Expression expr)
        {
            var call = (MethodCallExpression) (expr);
            return @ref.Tell(new Invocation(call.Method, EvaluateArguments(call)));
        }

        Task<TResult> CallResult<TResult>(Expression expr)
        {
            var call = (MethodCallExpression) (expr);
            return @ref.Ask<TResult>(new Invocation(call.Method, EvaluateArguments(call)));
        }

        static object[] EvaluateArguments(MethodCallExpression expression)
        {
            return expression.Arguments
                    .Select(arg => Expression.Lambda(arg).Compile().DynamicInvoke())
                    .ToArray();
        }

        public Task<TValue> Get<TValue>(Expression<Func<TActor, TValue>> expr)
        {
            Requires.NotNull(expr, "expr");

            var access = (MemberExpression)(expr.Body);
            return @ref.Ask<TValue>(new Invocation(access.Member));
        }

        public Task Set<TValue>(Expression<Func<TActor, TValue>> expr, TValue value)
        {
            Requires.NotNull(expr, "expr");

            var access = (MemberExpression)(expr.Body);
            return @ref.Tell(new Invocation(access.Member, new object[]{value}));
        }
    }

    public static class TypedExtensions
    {
        public static TypedActorRef<TActor> TypedActorOf<TActor>(this IActorSystem system, string id) where TActor : TypedActor
        {
            Requires.NotNull(system, "system");
            return new TypedActorRef<TActor>(system.ActorOf<TActor>(id));
        }

        public static TypedActorRef<TActor> Typed<TActor>(this ActorRef @ref) where TActor : TypedActor
        {
            Requires.NotNull(@ref, "ref");
            return new TypedActorRef<TActor>(@ref);
        }
    }
}