using System;

namespace Orleankka
{
    using Utility;

    [AttributeUsage(AttributeTargets.Interface)]
    public class ActorTypeAttribute : Attribute
    {
        internal readonly string Name;

        public ActorTypeAttribute(string name)
        {
            Requires.NotNullOrWhitespace(name, nameof(name));

            if (name.Contains(ActorPath.Separator[0]))
                throw new ArgumentException($"Actor type name cannot contain path separator: {name}");

            Name = name;
        }
    }
}