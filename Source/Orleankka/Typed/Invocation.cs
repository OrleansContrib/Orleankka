using System;
using System.Reflection;

namespace Orleankka.Typed
{
    [Serializable]
    sealed class Invocation
    {
        static readonly object[] NoArguments = new object[0];

        public string Member       { get; set; }
        public object[] Arguments  { get; set; }

        public Invocation()
        {
            Arguments = NoArguments;
        }

        public Invocation(MemberInfo member)
            : this(member, NoArguments)
        {}

        public Invocation(MemberInfo member, object[] arguments) 
            : this()
        {
            Member = member.Name;
            Arguments = arguments;
        }
    }
}