using System;
using System.Linq;

namespace FSM.Domain.Queries
{
    [Serializable]
    public class InventoryItemDetails
    {
        public readonly bool Active;
        public readonly string Name;
        public readonly int Total;

        public InventoryItemDetails(string name, int total, bool active)
        {
            Name = name;
            Total = total;
            Active = active;
        }
    }
}