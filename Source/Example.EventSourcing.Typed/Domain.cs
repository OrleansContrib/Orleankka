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

        public IEnumerable<Event> Create(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            if (this.name != null)
                throw new InvalidOperationException(
                    string.Format("Inventory item with id {0} has been already created", Id));

            yield return new InventoryItemCreated(name);
        }

        public IEnumerable<Event> Rename(string newName)
        {
            CheckIsActive();

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            yield return new InventoryItemRenamed(this.name, newName);
        }

        public IEnumerable<Event> CheckIn(int quantity)
        {
            CheckIsActive();

            if (quantity <= 0)
                throw new InvalidOperationException("must have a qty greater than 0 to add to inventory");

            yield return new InventoryItemCheckedIn(quantity);
        }

        public IEnumerable<Event> CheckOut(int quantity)
        {
            CheckIsActive();

            if (quantity <= 0)
                throw new InvalidOperationException("can't remove negative qty from inventory");

            yield return new InventoryItemCheckedOut(quantity);
        }

        public IEnumerable<Event> Deactivate()
        {
            CheckIsActive();

            yield return new InventoryItemDeactivated();
        }

        public InventoryItemDetails Details()
        {
            return new InventoryItemDetails(name, total, active);
        }

        void CheckIsActive()
        {
            if (!active)
                throw new InvalidOperationException(Id + " item is deactivated");
        }
    }

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
}
