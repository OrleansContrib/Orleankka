using System;
using System.Linq;

namespace Example
{
    public class CreateInventoryItem : Command
    {
        public string Name;
    }

    public class CheckInInventoryItem : Command
    {
        public int Quantity;
    }

    public class CheckOutInventoryItem : Command
    {
        public int Quantity;
    }

    public class RenameInventoryItem : Command
    {
        public string NewName;
    }

    public class DeactivateInventoryItem : Command
    {}

    public class InventoryItemCreated : Event
    {
        public string Name;
    }

    public class InventoryItemCheckedIn : Event
    {
        public int Quantity;
    }

    public class InventoryItemCheckedOut : Event
    {
        public int Quantity;
    }

    public class InventoryItemRenamed : Event
    {
        public string OldName;
        public string NewName;
    }

    public class InventoryItemDeactivated : Event
    {}

    public class GetInventoryItemDetails : Query<InventoryItemDetails>
    {}

    public class InventoryItemDetails
    {
        public string Name;
        public int Total;
        public bool Active;
    }
}
