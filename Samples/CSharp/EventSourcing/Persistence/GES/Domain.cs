﻿using System;
using System.Collections.Generic;

using Orleankka;
using Orleankka.Meta;

using Orleans;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;

namespace Example
{
    public interface IInventoryItem : IActorGrain, IGrainWithStringKey {}

    [MayInterleave(nameof(Interleave))]
    public class InventoryItem : EventSourcedActor, IInventoryItem
    {
        public static bool Interleave(IInvokable req) => req.Message() is GetDetails;

        int total;
        string name;
        bool active;

        void On(InventoryItemCreated e)
        {
            name = e.Name;
            active = true;
        }

        void On(InventoryItemRenamed e) => name = e.NewName;
        void On(InventoryItemCheckedIn e) => total += e.Quantity;
        void On(InventoryItemCheckedOut e) => total -= e.Quantity;
        void On(InventoryItemDeactivated e) => active = false;

        IEnumerable<Event> Handle(Create cmd)
        {
            if (string.IsNullOrEmpty(cmd.Name))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            if (name != null)
                throw new InvalidOperationException(
                    $"Inventory item with id {Id} has been already created");

            yield return new InventoryItemCreated(cmd.Name);
        }

        IEnumerable<Event> Handle(Rename cmd)
        {
            CheckIsActive();

            if (string.IsNullOrEmpty(cmd.NewName))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            yield return new InventoryItemRenamed(name, cmd.NewName);
        }

        IEnumerable<Event> Handle(CheckIn cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("must have a qty greater than 0 to add to inventory");

            yield return new InventoryItemCheckedIn(cmd.Quantity);
        }

        IEnumerable<Event> Handle(CheckOut cmd)
        {
            CheckIsActive();

            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("can't remove negative qty from inventory");

            yield return new InventoryItemCheckedOut(cmd.Quantity);
        }

        IEnumerable<Event> Handle(DeactivateItem cmd)
        {
            CheckIsActive();

            yield return new InventoryItemDeactivated();
        }

        InventoryItemDetails Handle(GetDetails query)
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
