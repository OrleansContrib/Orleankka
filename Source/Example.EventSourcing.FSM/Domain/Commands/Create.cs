using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Commands
{
    [Serializable]
    public class Create : Command<InventoryItem>
    {
        public readonly string Name;

        public Create(string name)
        {
            Name = name;
        }
    }
}