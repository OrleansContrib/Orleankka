using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Commands
{
    [Serializable]
    public class CheckIn : Command<InventoryItem>
    {
        public readonly int Quantity;

        public CheckIn(int quantity)
        {
            Quantity = quantity;
        }
    }
}