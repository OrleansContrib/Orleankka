using System;
using System.Linq;

using Orleankka.Meta;

namespace FSM.Domain.Commands
{
    [Serializable]
    public class Deactivate : Command<InventoryItem>
    {}
}