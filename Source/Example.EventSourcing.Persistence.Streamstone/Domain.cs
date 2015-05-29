using System;
using System.Collections.Generic;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    public class InventoryItem : EventSourcedActor
    {
        int total;
        string name;
        bool active;

        public InventoryItem()
        {}

        protected override void Define()
        {
            base.Define();

            On<InventoryItemCreated>(e =>
            {
                name = e.Name;
                active = true;
            });

            On<InventoryItemRenamed>(e => name = e.NewName);
            On<InventoryItemCheckedIn>(e => total += e.Quantity);
            On<InventoryItemCheckedOut>(e => total -= e.Quantity);
            On<InventoryItemDeactivated>(e => active = false);
        }

        public IEnumerable<Event> Handle(CreateInventoryItem cmd)
        {
            if (string.IsNullOrEmpty(cmd.Name))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            if (name != null)
                throw new InvalidOperationException(
                    string.Format("Inventory item with id {0} has been already created", Id));

            yield return new InventoryItemCreated(cmd.Name);
        }

        public IEnumerable<Event> Handle(RenameInventoryItem cmd)
        {
            CheckIsActive();

            if (string.IsNullOrEmpty(cmd.NewName))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            yield return new InventoryItemRenamed(name, cmd.NewName);
        }

        public IEnumerable<Event> Handle(CheckInInventoryItem cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("must have a qty greater than 0 to add to inventory");

            yield return new InventoryItemCheckedIn(cmd.Quantity);
        }

        public IEnumerable<Event> Handle(CheckOutInventoryItem cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("can't remove negative qty from inventory");

            yield return new InventoryItemCheckedOut(cmd.Quantity);
        }

        public IEnumerable<Event> Handle(DeactivateInventoryItem cmd)
        {
            CheckIsActive();

            yield return new InventoryItemDeactivated();
        }

        public InventoryItemDetails Handle(GetInventoryItemDetails query)
        {
            return new InventoryItemDetails(name, total, active);
        }

        void CheckIsActive()
        {
            if (!active)
                throw new InvalidOperationException(Id + " item is deactivated");
        }
    }
}
