using System;
using System.Linq;
using System.Threading.Tasks;

using Orleankka;

namespace Example
{
    [StreamSubscription(Source = "sms:/InventoryItem-.*/", Target = "#")]
    public class TotalItemsProjection : Actor
    {
        int totalItems = 0;
        public override Task OnActivate()
        {
            //Reread data from streams and dispatch like in EventSorcedActors
            return base.OnActivate();
        }

        public void Handle(DomainEvent<InventoryItemCheckedIn> ev) =>
            totalItems += ev.Event.Quantity;

        public void Handle(DomainEvent<InventoryItemCheckedOut> ev) =>
            totalItems -= ev.Event.Quantity;

        public int Answer(GetTotalItems qry) => totalItems;
    }
}