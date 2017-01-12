using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Commands
{
    [Serializable]
    public class Rename : Command
    {
        public readonly string NewName;

        public Rename(string newName)
        {
            NewName = newName;
        }
    }
}