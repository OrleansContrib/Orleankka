using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Events
{
    [Serializable]
    public class InventoryItemCheckedIn : Event
    {
        public readonly int Quantity;

        public InventoryItemCheckedIn(int quantity)
        {
            Quantity = quantity;
        }
    }
}