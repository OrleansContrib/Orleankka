using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Events
{
    [Serializable]
    public class InventoryItemCreated : Event
    {
        public readonly string Name;

        public InventoryItemCreated(string name)
        {
            Name = name;
        }
    }
}