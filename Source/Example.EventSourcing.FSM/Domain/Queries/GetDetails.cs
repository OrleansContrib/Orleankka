using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Queries
{
    [Serializable]
    public class GetDetails : Query<InventoryItem, InventoryItemDetails>
    {}
}