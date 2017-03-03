using System;

using Orleankka;
using Orleankka.Meta;
using Orleankka.Behaviors;

namespace Example
{
    [Reentrant(typeof(GetDetails))]
    public class InventoryItem : EventSourcedFsmActor
    {
        string name;
        int total;

        public InventoryItem() 
            : base(initial: nameof(Inactive))
        {}

        void On(InventoryItemCreated e)
        {
            name = e.Name;
            State = nameof(Active);
        }

        void On(InventoryItemRenamed e) => name = e.NewName;
        void On(InventoryItemCheckedIn e) => total += e.Quantity;
        void On(InventoryItemCheckedOut e) => total -= e.Quantity;
        void On(InventoryItemDeactivated e) => State = nameof(Deactivated);

        [Behavior] void Inactive()
        {
            this.OnReceive<GetDetails, InventoryItemDetails>(query => new InventoryItemDetails(name, total, active: false));

            OnReceive<Create>(cmd => 
            {
                if (string.IsNullOrEmpty(cmd.Name))
                    throw new ArgumentException("Inventory item name cannot be null or empty");

                if (name != null)
                    throw new InvalidOperationException(
                        $"Inventory item with id {Id} has been already created");

                return new InventoryItemCreated(cmd.Name) as Event;
            });
        }

        [Behavior] void Active()
        {
            this.OnReceive<GetDetails, InventoryItemDetails>(query => new InventoryItemDetails(name, total, active: true));
            
            OnReceive<CheckIn>(cmd =>
            {
                if (cmd.Quantity <= 0)
                    throw new InvalidOperationException("must have a qty greater than 0 to add to inventory");

                return new InventoryItemCheckedIn(cmd.Quantity);
            });

            OnReceive<CheckOut>(cmd =>
            {
                if (cmd.Quantity <= 0)
                    throw new InvalidOperationException("can't remove negative qty from inventory");

                return new InventoryItemCheckedOut(cmd.Quantity);
            });

            OnReceive<Rename>(cmd =>
            {
                if (string.IsNullOrEmpty(cmd.NewName))
                    throw new ArgumentException("Inventory item name cannot be null or empty");

                return new InventoryItemRenamed(name, cmd.NewName);
            });

            OnReceive<Deactivate>(cmd => new InventoryItemDeactivated());
        }

        [Behavior] void Deactivated()
        {
            this.OnReceive(_ => { throw new InvalidOperationException(Id + " item is deactivated"); });
        }
    }
}