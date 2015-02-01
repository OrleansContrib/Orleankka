using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleankka.Core.Hardcore
{
    public static class Mixer
    {
        public static readonly IEnumerable<Blend> AllBlends;
        public static readonly IEnumerable<Blend> AllowedBlends;

        static Mixer()
        {
            var categories = new[]
            {
                Category.Of<Activation>(),
                Category.Of<Concurrency>(),
                Category.Of<Placement>(),
                Category.Of<Delivery>(),
                Category.Of<Interleave>()
            };

            var blends = new List<Blend>();
            Mix(categories, Blend.Default, blends);

            AllBlends = blends.Distinct().ToList();
            AllowedBlends  = AllBlends.Where(Recipe.IsValid).ToList();
        }

        static void Mix(ICollection<Category> categories, Blend previous, ICollection<Blend> result)
        {
            if (categories.Count == 0)
                return;

            var category = categories.First();
            var blends = category.Select(previous.Mix);

            foreach (var blend in blends)
            {
                result.Add(blend);
                
                var next = categories
                    .Skip(1)
                    .ToArray();

                Mix(next, blend, result);
            }
        }
    }
}
