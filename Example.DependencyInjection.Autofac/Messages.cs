using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class CreateInventoryItem : Command
    {
        public readonly string Name;

        public CreateInventoryItem(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class CheckInInventoryItem : Command
    {
        public readonly int Quantity;

        public CheckInInventoryItem(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class CheckOutInventoryItem : Command
    {
        public readonly int Quantity;

        public CheckOutInventoryItem(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class RenameInventoryItem : Command
    {
        public readonly string NewName;

        public RenameInventoryItem(string newName)
        {
            NewName = newName;
        }
    }
	
    [Serializable]
    public class DeactivateInventoryItem : Command
    {}

    [Serializable]
    public class GetInventoryItemDetails : Query<InventoryItemDetails>
    {}

    [Serializable]
    public class InventoryItemDetails
    {
        public readonly string Name;
        public readonly int Total;
        public readonly bool Active;

        public InventoryItemDetails(string name, int total, bool active)
        {
            Name = name;
            Total = total;
            Active = active;
        }
    }
	
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
