using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
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

    [Serializable]
    public class InventoryItemCheckedIn : Event
    {
        public readonly int Quantity;

        public InventoryItemCheckedIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class InventoryItemCheckedOut : Event
    {
        public readonly int Quantity;

        public InventoryItemCheckedOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class InventoryItemRenamed : Event
    {
        public readonly string OldName;
        public readonly string NewName;

        public InventoryItemRenamed(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    [Serializable]
    public class InventoryItemDeactivated : Event
    {}
}
