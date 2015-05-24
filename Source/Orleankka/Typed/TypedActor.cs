using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka.Typed
{
    public abstract class TypedActor : Actor
    {
        static readonly Task<object> Done = Task.FromResult((object)null);

        static readonly Dictionary<Type, Dictionary<string, MemberInfo>> cache =
                    new Dictionary<Type, Dictionary<string, MemberInfo>>();

        protected internal override void Define()
        {
            if (GetType().IsAbstract)
                return;

            var members = new Dictionary<string, MemberInfo>();
            
            foreach (var member in GetType().GetMembers())
            {
                if (members.ContainsKey(member.Name))
                {
                    var message = "Typed actors have bind-by-name semantics." + 
                                  "Public members with the same name are not allowed:\n" + 
                                  string.Format("Type: {0}, Member: {1}", GetType(), member.Name);

                    throw new InvalidOperationException(message);
                }

                members.Add(member.Name, member);
            }

            cache.Add(GetType(), members);
        }

        protected internal override Task<object> OnReceive(object message)
        {
            var invocation = message as Invocation;
            if (invocation == null)
                throw new InvalidOperationException("Only member invocations could be sent to a typed actors");

            var members = cache.Find(GetType());
            var member = members != null 
                ? members.Find(invocation.Member) 
                : null;

            if (member == null)
                throw new InvalidOperationException(
                    string.Format("Can't find member registration for typed actor {0}." +
                                  "Make sure that you've registered assembly containing this type", GetType()));

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