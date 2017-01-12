using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FSM.Domain.Commands;
using FSM.Domain.Events;
using FSM.Domain.Queries;
using FSM.Infrastructure;

using Orleankka;
using Orleankka.Behaviors;
using Orleankka.Meta;

namespace FSM.Domain
{
    [Reentrant(typeof(GetDetails))]
    public class InventoryItem : EventSourcedActor
    {
        private string name;
        private int total;

        private void Setup(string behavior)
        {
            this.OnBecome(() => Console.WriteLine($"OnBecome_{behavior}"));
            this.OnUnbecome(() => Console.WriteLine($"OnUnbecome_{behavior}"));
            this.OnActivate(() => Console.WriteLine($"OnActivate_{behavior}"));
            this.OnDeactivate(() => Console.WriteLine($"OnDeactivate_{behavior}"));
            this.OnReceive<GetDetails, InventoryItemDetails>( query => Answer(query));
        }

        public override Task OnActivate()
        {
            Behavior.Initial(Inactive);
            return base.OnActivate();
        }

        private InventoryItemDetails Answer(GetDetails _)
        {
            return new InventoryItemDetails(name, total, Behavior.Current == nameof(Active));
        }

        private void On(InventoryItemCreated e)
        {
            name = e.Name;
            this.Become(Active);
        }

        private void On(InventoryItemRenamed e) => name = e.NewName;
        private void On(InventoryItemCheckedIn e) => total += e.Quantity;
        private void On(InventoryItemCheckedOut e) => total -= e.Quantity;
        private void On(InventoryItemDeactivated e) => this.Become(Deactivated);

        private IEnumerable<Event> Handle(Create cmd)
        {
            if (string.IsNullOrEmpty(cmd.Name))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            if (name != null)
                throw new InvalidOperationException(
                    $"Inventory item with id {Id} has been already created");

            yield return new InventoryItemCreated(cmd.Name);
        }

        private void Inactive()
        {
            Setup(nameof(Inactive));
            Behavior.OnReceive<Create, IEnumerable<Event>>(cmd => Handle(cmd));
        }

        private void Active()
        {
            Setup(nameof(Active));
            this.OnReceive<CheckIn, IEnumerable<Event>>(cmd => Handle(cmd));
            this.OnReceive<CheckOut, IEnumerable<Event>>(cmd => Handle(cmd));
            this.OnReceive<Rename, IEnumerable<Event>>(cmd => Handle(cmd));
            this.OnReceive<Deactivate, IEnumerable<Event>>(cmd => Handle(cmd));
        }

        private IEnumerable<Event> Handle(Deactivate cmd)
        {
            yield return new InventoryItemDeactivated();
        }

        private IEnumerable<Event> Handle(Rename cmd)
        {
            if (string.IsNullOrEmpty(cmd.NewName))
                throw new ArgumentException("Inventory item name cannot be null or empty");

            yield return new InventoryItemRenamed(name, cmd.NewName);
        }

        private IEnumerable<Event> Handle(CheckOut cmd)
        {
            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("can't remove negative qty from inventory");

            yield return new InventoryItemCheckedOut(cmd.Quantity);
        }

        private IEnumerable<Event> Handle(CheckIn cmd)
        {
            if (cmd.Quantity <= 0)
                throw new InvalidOperationException("must have a qty greater than 0 to add to inventory");

            yield return new InventoryItemCheckedIn(cmd.Quantity);
        }

        private void Deactivated()
        {
            Setup(nameof(Deactivated));
            this.OnReceive(_ => { throw new InvalidOperationException(Id + " item is deactivated"); });
        }
    }
}