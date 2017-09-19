using Orleankka;
using Orleankka.Meta;

namespace Example
{
    public class Create : Command
    {
        public string Name;
    }

    public class CheckIn : Command
    {
        public int Quantity;
    }

    public class CheckOut : Command
    {
        public int Quantity;
    }

    public class Rename : Command
    {
        public string NewName;
    }

    public class Deactivate : Command
    {}

    public class GetDetails : Query<InventoryItemDetails>
    {}

    public class InventoryItemDetails : Result
    {
        public string Name;
        public int Total;
        public bool Active;
    }
}
