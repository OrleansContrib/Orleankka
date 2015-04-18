using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleankka.Typed
{
    public class TypedActor : Actor
    {
        public override Task<object> OnReceive(object message)
        {
            var invocation = message as Invocation;
        
            if (invocation == null)
                throw new ArgumentException("Only member invocations could be sent to a typed actors", "message");

            var member = GetType()
                .GetMembers()
                .Single(x => x.MetadataToken == invocation.Token);

            if (member.MemberType == MemberTypes.Method)
                return InvokeMethod(member as MethodInfo, invocation.Arguments);

            throw new NotSupportedException("Yet");
        }

        async Task<object> InvokeMethod(MethodInfo method, object[] arguments)
        {
            var result = method.Invoke(this, arguments);

            if (!typeof(Task).IsAssignableFrom(method.ReturnType))
                return method.ReturnType == typeof(void) ? null : result;

            if (method.ReturnType.GenericTypeArguments.Length != 0)
                throw new NotSupportedException("Yet");
            
            await (Task) result;
            return null;
        }
    }
}