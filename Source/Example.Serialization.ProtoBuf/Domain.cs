using System;

using Orleankka;

namespace Example
{
    public interface IInventoryItem : IActor
    { }

    public class InventoryItem : Actor, IInventoryItem
    {
        int total;
        string name;
        bool active;

        InventoryItemDetails Handle(GetDetails _)
        {
            CheckCreated();

            return new InventoryItemDetails
            {
                Name = name,
                Total = total,
                Active = active
            };
        }

        void Handle(Create cmd)
        {
            CheckNotCreated();

            if (string.IsNullOrEmpty(cmd.Name))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            name = cmd.Name;
            active = true;
        }

        void Handle(Rename cmd)
        {
            CheckIsActive();

            if (string.IsNullOrEmpty(cmd.NewName))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            name = cmd.NewName;
        }

        void Handle(CheckIn cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("Must have quantity greater than 0 to add to inventory item stock");

            total += cmd.Quantity;
        }

        void Handle(CheckOut cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("Can't remove negative quantity from inventory item stock");

            total -= cmd.Quantity;
        }

        void Handle(Deactivate cmd)
        {
            CheckIsActive();

            active = false;
        }

        void CheckIsActive()
        {
            CheckCreated();

            if (!active)
                throw new InventoryItemException($"Inventory item with id [{Id}] is not active anymore");
        }

        void CheckCreated()
        {
            if (name == null)
                throw new InventoryItemException($"Inventory item with id [{Id}] has not been created");
        }

        void CheckNotCreated()
        {
            if (name != null)
                throw new InvalidOperationException($"Inventory item with id [{Id}] has been already created");
        }
    }
}
