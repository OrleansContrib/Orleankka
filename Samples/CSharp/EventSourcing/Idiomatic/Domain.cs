using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleankka;
using Orleankka.Meta;

using Orleans;
using Orleans.CodeGeneration;
using Orleans.Concurrency;
using Orleans.Streams;

namespace Example
{
    public interface IInventoryItem : IActorGrain
    {}

    [MayInterleave(nameof(Interleave))]
    public class InventoryItem : EventSourcedActor, IInventoryItem
    {
        public static bool Interleave(InvokeMethodRequest req) => req.Message() is GetDetails;

        int total;
        string name;
        bool active;

        void On(InventoryItemCreated e)
        {
            name = e.Name;
            active = true;
        }

        void On(InventoryItemRenamed e)     => name = e.NewName;
        void On(InventoryItemCheckedIn e)   => total += e.Quantity;
        void On(InventoryItemCheckedOut e)  => total -= e.Quantity;
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

    public interface IInventoryDispatcher : IActorGrain
    {}

    public interface IInventory : IActorGrain
    { }

    [StartsWithImplicitStreamSubscription("InventoryItem")]
    public class InventoryDispatcher : DispatchActorGrain, IInventoryDispatcher, IGrainWithGuidCompoundKey
    {
        async Task On(Activate _)
        {
            var streamProvider = GetStreamProvider("sms");

            var guid = this.GetPrimaryKey(out var extension);
            var stream = streamProvider.GetStream<IEventEnvelope>(guid, extension);

            await stream.SubscribeAsync((envelope, token) => Receive(envelope));
        }

        async Task On(EventEnvelope<InventoryItemCreated> e) => await System.ActorOf<IInventory>("#").Tell(new TrackStockOfNewInventoryItem(e.Stream, e.Event.Name));
        async Task On(EventEnvelope<InventoryItemCheckedIn> e) => await System.ActorOf<IInventory>("#").Tell(new IncrementStockLevel(e.Stream, e.Event.Quantity));
        async Task On(EventEnvelope<InventoryItemCheckedOut> e) => await System.ActorOf<IInventory>("#").Tell(new DecrementStockLevel(e.Stream, e.Event.Quantity));
        async Task On(EventEnvelope<InventoryItemDeactivated> e) => await System.ActorOf<IInventory>("#").Tell(new DiscontinueItem(e.Stream));
        async Task On(EventEnvelope<InventoryItemRenamed> e) => await System.ActorOf<IInventory>("#").Tell(new RenameItem(e.Stream, e.Event.NewName));
    }

    public class Inventory : EventSourcedActor, IInventory
    {
        readonly Dictionary<string, InventoryItemDetails> items = new Dictionary<string, InventoryItemDetails>();

        public IEnumerable<Event> Handle(TrackStockOfNewInventoryItem cmd)
        {
            yield return new StockOfNewInventoryItemTracked(cmd.Id, cmd.Name);
        }

        public IEnumerable<Event> Handle(IncrementStockLevel cmd)
        {
            yield return new StockLevelIncremented(cmd.Id, cmd.Quantity);
        }

        public IEnumerable<Event> Handle(DecrementStockLevel cmd)
        {
            yield return new StockLevelDecremented(cmd.Id, cmd.Quantity);
        }

        public IEnumerable<Event> Handle(DiscontinueItem cmd)
        {
            yield return new ItemDiscontinued(cmd.Id);
        }

        public IEnumerable<Event> Handle(RenameItem cmd)
        {
            yield return new ItemRenamed(cmd.Id, cmd.Name);
        }

        public void On(StockOfNewInventoryItemTracked e) => items[e.Id] = new InventoryItemDetails(e.Name, 0, true);
        public void On(StockLevelIncremented e) => items[e.Id].Total += e.Quantity;
        public void On(StockLevelDecremented e) => items[e.Id].Total -= e.Quantity;
        public void On(ItemDiscontinued e) => items[e.Id].Active = false;
        public void On(ItemRenamed e) => items[e.Id].Name = e.Name;

        InventoryItemDetails[] Answer(GetInventoryItems _) => items.Values.ToArray();
        int Answer(GetInventoryItemsTotal _) => items.Values.Sum(x => x.Total);
    }

    public class StartsWithPredicate : IStreamNamespacePredicate
    {
        readonly string startsWith;

        public StartsWithPredicate(string startsWith) => this.startsWith = startsWith;

        public bool IsMatch(string streamNamespace) => streamNamespace.StartsWith(startsWith);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StartsWithImplicitStreamSubscriptionAttribute : ImplicitStreamSubscriptionAttribute
    {
        public StartsWithImplicitStreamSubscriptionAttribute(string startsWith)
            : base(new StartsWithPredicate(startsWith))
        { }
    }
}
