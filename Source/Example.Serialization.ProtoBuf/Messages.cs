using Orleankka.Meta;

namespace Example
{
    public partial class Create        : Command {}
    public partial class Rename        : Command {}
    public partial class CheckIn       : Command {}
    public partial class CheckOut      : Command {}
    public partial class DeactivateItem: Command {}
    public partial class GetDetails    : Query<InventoryItemDetails> {}
}
