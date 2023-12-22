using System;

using Orleankka.Meta;

using Orleans;

namespace Example
{
    [Serializable, GenerateSerializer]
    public class TrackStockOfNewInventoryItem : Command
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly string Name;

        public TrackStockOfNewInventoryItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class IncrementStockLevel : Command
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly int Quantity;

        public IncrementStockLevel(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class DecrementStockLevel : Command
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly int Quantity;

        public DecrementStockLevel(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class DiscontinueItem : Command
    {
        [Id(0)]
        public readonly string Id;

        public DiscontinueItem(string id)
        {
            Id = id;
        }
    }

    [Serializable, GenerateSerializer]
    public class RenameItem : Command
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly string Name;

        public RenameItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class Create : Command
    {
        [Id(0)]
        public readonly string Name;

        public Create(string name)
        {
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class CheckIn : Command
    {
        [Id(0)]
        public readonly int Quantity;

        public CheckIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class CheckOut : Command
    {
        [Id(0)]
        public readonly int Quantity;

        public CheckOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class Rename : Command
    {
        [Id(0)]
        public readonly string NewName;

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
    public class GetInventoryItems : Query<InventoryItemDetails[]>
    {}

    [Serializable, GenerateSerializer]
    public class GetInventoryItemsTotal : Query<int>
    {}

    public interface IEventEnvelope
    {

    }

    [Serializable, GenerateSerializer]
    public class EventEnvelope<T> : IEventEnvelope where T : Event
    {
        [Id(0)]
        public string Stream { get; }
        [Id(1)]
        public T Event { get; }

        public EventEnvelope(string stream, T @event)
        {
            Stream = stream;
            Event = @event;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemDetails
    {
        [Id(0)]
        public string Name;
        [Id(1)]
        public int Total;
        [Id(2)]
        public bool Active;

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
        [Id(0)]
        public readonly string Name;

        public InventoryItemCreated(string name)
        {
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemCheckedIn : Event
    {
        [Id(0)]
        public readonly int Quantity;

        public InventoryItemCheckedIn(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemCheckedOut : Event
    {
        [Id(0)]
        public readonly int Quantity;

        public InventoryItemCheckedOut(int quantity)
        {
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemRenamed : Event
    {
        [Id(0)]
        public readonly string OldName;
        [Id(1)]
        public readonly string NewName;

        public InventoryItemRenamed(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    [Serializable, GenerateSerializer]
    public class InventoryItemDeactivated : Event
    {}

    [Serializable, GenerateSerializer]
    public class StockOfNewInventoryItemTracked : Event
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly string Name;

        public StockOfNewInventoryItemTracked(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    [Serializable, GenerateSerializer]
    public class StockLevelIncremented : Event
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly int Quantity;

        public StockLevelIncremented(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class StockLevelDecremented : Event
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly int Quantity;

        public StockLevelDecremented(string id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable, GenerateSerializer]
    public class ItemDiscontinued : Event
    {
        [Id(0)]
        public readonly string Id;

        public ItemDiscontinued(string id)
        {
            Id = id;
        }
    }

    [Serializable, GenerateSerializer]
    public class ItemRenamed : Event
    {
        [Id(0)]
        public readonly string Id;
        [Id(1)]
        public readonly string Name;

        public ItemRenamed(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
