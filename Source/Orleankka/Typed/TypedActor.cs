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

        public override Task<object> OnReceive(object message)
        {
            var invocation = message as Invocation;
        
            if (invocation == null)
                throw new ArgumentException("Only member invocations could be sent to a typed actors", "message");

            var member = GetType()
                .GetMembers()
                .Single(x => x.MetadataToken == invocation.Token);

            return member.MemberType == MemberTypes.Method 
                    ? InvokeMethod(member as MethodInfo, invocation.Arguments) 
                    : InvokeMember(member, invocation.Arguments);
        }

        Task<object> InvokeMethod(MethodInfo method, object[] arguments)
        {
            var result = method.Invoke(this, arguments);

            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
                return method.ReturnType != typeof(void) ? Task.FromResult(result) : Done;

            var task = (Task) result;
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

        Task<object> InvokeMember(MemberInfo member, object[] arguments)
        {
            if (member.MemberType == MemberTypes.Field)
                throw new NotSupportedException("Yet");

            var property = (PropertyInfo) member;
            if (arguments.Length == 0)
                return Task.FromResult(property.GetValue(this));

            property.SetValue(this, arguments[0]);
            return Done;
        }
    }
}