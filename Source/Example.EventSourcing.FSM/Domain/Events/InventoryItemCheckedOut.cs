using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Events
{
    [Serializable]
    public class InventoryItemCheckedOut : Event
    {
        public readonly int Quantity;

        public InventoryItemCheckedOut(int quantity)
        {
            Quantity = quantity;
        }
    }
}