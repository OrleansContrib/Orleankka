using System;
using System.Reflection;

namespace Orleankka.Typed
{
    [Serializable]
    public sealed class Invocation
    {
        static readonly object[] NoArguments = new object[0];

        [NonSerialized]
        public readonly MemberInfo Member;

        public int Token            { get; set; }
        public object[] Arguments   { get; set; }

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
            Member = member;
            Token  = member.MetadataToken;
            Arguments = arguments;
        }
    }
}