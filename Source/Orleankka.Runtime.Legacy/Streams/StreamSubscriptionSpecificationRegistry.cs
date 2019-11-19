using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Legacy.Streams
{
    using Utility;

    class StreamSubscriptionSpecificationRegistry
    {
        readonly Dictionary<string, List<StreamSubscriptionSpecification>> providerSubscriptions = 
             new Dictionary<string, List<StreamSubscriptionSpecification>>();

        internal void Register(IEnumerable<StreamSubscriptionSpecification> specifications)
        {
            foreach (var each in specifications)
            {
                var provider = providerSubscriptions.Find(each.Provider);
                if (provider == null)
                {
                    provider = new List<StreamSubscriptionSpecification>();
                    providerSubscriptions.Add(each.Provider, provider);
                }

                provider.Add(each);
            }
        }

        internal IEnumerable<StreamSubscriptionSpecification> Find(string provider) => 
            providerSubscriptions.Find(provider) ?? Enumerable.Empty<StreamSubscriptionSpecification>();
    }
}