using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;
using Orleankka.Meta;

namespace Example
{
    [Reentrant(typeof(GetTotalItems))]
    [StreamSubscription(Source = "sms:/InventoryItem-.*/", Target = "#")]
    public class TotalItemsProjection : Actor
    {
        int totalItems = 0;
        public override Task OnActivate()
        {
            //Read data from persistent storage and dispatch like in EventSorcedActors
            return base.OnActivate();
        }

        public void Handle(DomainEvent<InventoryItemCheckedIn> ev) =>
            totalItems += ev.Event.Quantity;

        public void Handle(DomainEvent<InventoryItemCheckedOut> ev) =>
            totalItems -= ev.Event.Quantity;

        public int Answer(GetTotalItems qry) => totalItems;
    }

    [Serializable]
    public class GetTotalItems : Query<TotalItemsProjection, int>
    { }
}