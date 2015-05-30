using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Orleankka.Utility;

namespace Orleankka.Typed
{
    public abstract class TypedActor : Actor<TypedActorPrototype>
    {
        protected TypedActor()
        {}

        protected TypedActor(string id, IActorSystem system)
            : base(id, system)
        {}

        protected internal override Task<object> OnReceive(object message)
        {
            var invocation = message as Invocation;
            if (invocation == null)
                throw new InvalidOperationException("Only member invocations could be sent to a typed actors");

            var member = _.Member(invocation.Member);
            return OnInvoke(member, invocation.Arguments);
        }

        protected virtual Task<object> OnInvoke(MemberInfo member, object[] arguments)
        {
            return TypedActorPrototype.Invoke(this, member, arguments);
        }
    }

    public class TypedActorPrototype : ActorPrototype
    {
        static readonly Task<object> Done = Task.FromResult((object)null);

        readonly Dictionary<string, MemberInfo> members = 
             new Dictionary<string, MemberInfo>();

        public TypedActorPrototype(Type actor)
            : base(actor)
        {
            foreach (var member in actor.GetMembers())
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
        }

        public MemberInfo Member(string name)
        {
            var member = members.Find(name);

            if (member == null)
                throw new InvalidOperationException(
                    string.Format("Can't find member registration for typed actor {0}." +
                                  "Make sure that you've registered assembly containing this type", GetType()));
            
            return member;
        }

        public static Task<object> Invoke(TypedActor target, MemberInfo member, object[] arguments)
        {
            return member.MemberType == MemberTypes.Method
                       ? DoInvoke(target, (MethodInfo)member, arguments)
                       : DoInvoke(target, member, arguments);
        }

        static Task<object> DoInvoke(TypedActor target, MethodInfo method, object[] arguments)
        {
            var result = method.Invoke(target, arguments);

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
                        ? (object)((dynamic)task).Result
                        : (object)null;
            },
            method.ReturnType);
        }

        static Task<object> DoInvoke(TypedActor target, MemberInfo member, object[] arguments)
        {
            return member.MemberType == MemberTypes.Field
                    ? DoInvoke(target, (FieldInfo)member, arguments)
                    : DoInvoke(target, (PropertyInfo)member, arguments);
        }

        static Task<object> DoInvoke(TypedActor target, FieldInfo field, object[] arguments)
        {
            if (arguments.Length == 0)
                return Task.FromResult(field.GetValue(target));

            field.SetValue(target, arguments[0]);
            return Done;
        }

        static Task<object> DoInvoke(TypedActor target, PropertyInfo property, object[] arguments)
        {
            if (arguments.Length == 0)
                return Task.FromResult(property.GetValue(target));

            property.SetValue(target, arguments[0]);
            return Done;
        }
    }
}