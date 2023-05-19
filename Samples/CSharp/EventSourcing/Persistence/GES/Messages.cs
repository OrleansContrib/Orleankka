using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    using Orleans;

    [Serializable, GenerateSerializer]
    public class Create : Command
    {
        [Id(0)] public readonly string Name;

        public Create(string name)
        {
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class CheckIn : Command
    {
        [Id(0)] public readonly int Quantity;

        public CheckIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class CheckOut : Command
    {
        [Id(0)] public readonly int Quantity;

        public CheckOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class Rename : Command
    {
        [Id(0)] public readonly string NewName;

        public Rename(string newName)
        {
            NewName = newName;
        }
    }

    [Serializable, GenerateSerializer]
    public class DeactivateItem : Command
    {}

    [Serializable, GenerateSerializer]
    public class GetDetails : Query<InventoryItemDetails>
    {}

    [Serializable, GenerateSerializer]
    public class InventoryItemDetails
    {
        [Id(0)] public readonly string Name;
        [Id(1)] public readonly int Total;
        [Id(2)] public readonly bool Active;

        public InventoryItemDetails(string name, int total, bool active)
        {
            Name = name;
            Total = total;
            Active = active;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemCreated : Event
    {
        [Id(0)] public readonly string Name;

        public InventoryItemCreated(string name)
        {
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemCheckedIn : Event
    {
        [Id(0)] public readonly int Quantity;

        public InventoryItemCheckedIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemCheckedOut : Event
    {
        [Id(0)] public readonly int Quantity;

        public InventoryItemCheckedOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemRenamed : Event
    {
        [Id(0)] public readonly string OldName;
        [Id(1)] public readonly string NewName;

        public InventoryItemRenamed(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemDeactivated : Event
    {}
}
