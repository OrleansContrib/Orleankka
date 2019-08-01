using System;

using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class TrackStockOfNewInventoryItem : Command
    {
        public readonly string Id;
        public readonly string Name;

        public TrackStockOfNewInventoryItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [Serializable]
    public class IncrementStockLevel : Command
    {
        public readonly string Id;
        public readonly int Quantity;

        public IncrementStockLevel(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class DecrementStockLevel : Command
    {
        public readonly string Id;
        public readonly int Quantity;

        public DecrementStockLevel(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class DiscontinueItem : Command
    {
        public readonly string Id;

        public DiscontinueItem(string id)
        {
            Id = id;
        }
    }

    [Serializable]
    public class RenameItem : Command
    {
        public readonly string Id;
        public readonly string Name;

        public RenameItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

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
    public class DeactivateItem : Command
    {}

    [Serializable]
    public class GetDetails : Query<InventoryItemDetails>
    {}

    [Serializable]
    public class GetInventoryItems : Query<InventoryItemDetails[]>
    {}

    [Serializable]
    public class GetInventoryItemsTotal : Query<int>
    {}

    public interface IEventEnvelope
    {

    }

    [Serializable]
    public class EventEnvelope<T> : IEventEnvelope where T : Event
    {
        public string Stream { get; }
        public T Event { get; }

        public EventEnvelope(string stream, T @event)
        {
            Stream = stream;
            Event = @event;
        }
    }

    [Serializable]
    public class InventoryItemDetails
    {
        public string Name;
        public int Total;
        public bool Active;

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

    [Serializable]
    public class StockOfNewInventoryItemTracked : Event
    {
        public readonly string Id;
        public readonly string Name;

        public StockOfNewInventoryItemTracked(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [Serializable]
    public class StockLevelIncremented : Event
    {
        public readonly string Id;
        public readonly int Quantity;

        public StockLevelIncremented(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class StockLevelDecremented : Event
    {
        public readonly string Id;
        public readonly int Quantity;

        public StockLevelDecremented(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class ItemDiscontinued : Event
    {
        public readonly string Id;

        public ItemDiscontinued(string id)
        {
            Id = id;
        }
    }

    [Serializable]
    public class ItemRenamed : Event
    {
        public readonly string Id;
        public readonly string Name;

        public ItemRenamed(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
