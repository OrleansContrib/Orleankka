using System;
using System.Linq;

namespace Orleankka
{
    public struct Activate
    {}

    public struct Deactivate
    {}

    struct Reminder
    {
        public readonly string Name;

        public Reminder(string name)
        {
            Name = name;
        }
    }
}