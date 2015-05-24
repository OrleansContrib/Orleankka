using System;
using System.Reflection;

namespace Orleankka.Typed
{
    [Serializable]
    public sealed class Invocation
    {
        static readonly object[] NoArguments = new object[0];

        public int ModuleToken      { get; set; }
        public int MemberToken      { get; set; }
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
            ModuleToken = member.Module.MetadataToken;
            MemberToken = member.MetadataToken;
            Arguments = arguments;
        }
    }
}