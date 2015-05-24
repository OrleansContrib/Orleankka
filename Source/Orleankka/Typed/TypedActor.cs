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
                .Single(x => 
                    x.Module.MetadataToken == invocation.ModuleToken && 
                    x.MetadataToken == invocation.MemberToken);

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
                return method.ReturnType != typeof(void) 
                        ? Task.FromResult(result) 
                        : Done;

            return ((Task)result).ContinueWith((task, state) =>
            {
                if (task.Status == TaskStatus.Faulted)
                {
                    Debug.Assert(task.Exception != null);
                    throw task.Exception;
                }

                var returnType = (Type)state;
                var returnsResult = returnType != typeof(Task);

                return returnsResult 
                        ? (object) ((dynamic) task).Result 
                        : (object) null;
            },
            method.ReturnType);
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