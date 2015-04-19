using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Typed
{
    public class TypedActor : Actor
    {
        static readonly Task<object> Done = Task.FromResult((object)null);

        protected internal override Task<object> OnReceive(object message)
        {
            var invocation = message as Invocation;

            if (invocation == null)
                throw new ArgumentException("Only member invocations could be sent to a typed actors", "message");

            var member = GetType()
                .GetMembers()
                .Single(x => x.MetadataToken == invocation.Token);

            return OnInvoke(member, invocation.Arguments);
        }

        protected virtual Task<object> OnInvoke(MemberInfo member, object[] arguments)
        {
            return member.MemberType == MemberTypes.Method
                       ? Invoke((MethodInfo) member, arguments)
                       : Invoke(member, arguments);
        }

        Task<object> Invoke(MethodInfo method, object[] arguments)
        {
            var result = method.Invoke(this, arguments);

            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
                return method.ReturnType != typeof(void) ? Task.FromResult(result) : Done;

            var task = (Task)result;
            return task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.Faulted)
                {
                    Debug.Assert(t.Exception != null);
                    throw t.Exception;
                }

                if (t.GetType() == typeof(Task))
                    return (object)null;

                return (object)((dynamic)task).Result;
            });
        }

        Task<object> Invoke(MemberInfo member, object[] arguments)
        {
            return member.MemberType == MemberTypes.Field
                    ? Invoke((FieldInfo)member, arguments)
                    : Invoke((PropertyInfo)member, arguments);
        }

        Task<object> Invoke(FieldInfo field, object[] arguments)
        {
            if (arguments.Length == 0)
                return Task.FromResult(field.GetValue(this));

            field.SetValue(this, arguments[0]);
            return Done;
        }

        Task<object> Invoke(PropertyInfo property, object[] arguments)
        {
            if (arguments.Length == 0)
                return Task.FromResult(property.GetValue(this));

            property.SetValue(this, arguments[0]);
            return Done;
        }
    }
}