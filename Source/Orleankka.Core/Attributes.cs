using System;
using System.Linq;

namespace Orleankka
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DeliveryOrderAgnosticAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EvenDistributionPlacementAttribute : Attribute
    {
        // IMHO, would be a better name for ActivationCountBasedPlacementAttribute
        // Because, it tells about WHAT, and not about HOW
    }
}
