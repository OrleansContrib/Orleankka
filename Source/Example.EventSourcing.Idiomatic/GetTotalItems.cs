using System;
using System.Linq;

using Orleankka.Meta;

namespace Example
{
    [Serializable]
    public class GetTotalItems:Query<TotalItemsProjection,int>
    {}
}