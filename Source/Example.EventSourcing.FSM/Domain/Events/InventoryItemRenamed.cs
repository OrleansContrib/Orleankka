using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Events
{
    [Serializable]
    public class InventoryItemRenamed : Event
    {
        public readonly string NewName;
        public readonly string OldName;

        public InventoryItemRenamed(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }
}