using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class Create : Command
    {
        public readonly string Name;

        public Create(string name)
        {
            Name = name;
        }
    }

    [Serializable]
    public class CheckIn : Command
    {
        public readonly int Quantity;

        public CheckIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class CheckOut : Command
    {
        public readonly int Quantity;

        public CheckOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable]
    public class Rename : Command
    {
        public readonly string NewName;

        public Rename(string newName)
        {
            NewName = newName;
        }
    }

    [Serializable]
    public class Deactivate : Command
    {}

    [Serializable]
    public class GetDetails : Query<InventoryItemDetails>
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
