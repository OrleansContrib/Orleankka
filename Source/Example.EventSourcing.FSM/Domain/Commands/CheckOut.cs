using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Commands
{
    [Serializable]
    public class CheckOut : Command<InventoryItem>
    {
        public readonly int Quantity;

        public CheckOut(int quantity)
        {
            Quantity = quantity;
        }
    }
}