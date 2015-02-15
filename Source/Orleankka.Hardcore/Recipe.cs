using System;
using System.Collections.Generic;
using System.Linq;

using Orleans.Concurrency;
using Orleans.Placement;

namespace Orleankka.Core.Hardcore
{
    public abstract class Recipe
    {
        static readonly List<Recipe> wellKnownRecipes = new List<Recipe>
        {
            new StatelessWorkerCannotSpecifyPlacement()
        };

        public static bool IsValid(Blend blend)
        {
            return wellKnownRecipes.All(recipe => recipe.Validate(blend) == null);
        }

        public static void AssertValid(Blend blend, Type type)
        {
            var errors = wellKnownRecipes
                .Select(recipe => recipe.Validate(blend))
                .Where(err => err != null)
                .ToArray();
            
            if (errors.Any())
                throw new ApplicationException(
                    string.Format("Type {0} has invalid mix of actor configuration options: {1}", type, errors.First()));
        }

        protected abstract string Validate(Blend blend);
    }

    public class StatelessWorkerCannotSpecifyPlacement : Recipe
    {
        protected override string Validate(Blend blend)
        {
            bool isStatelessWorker = blend.Flavors
                .Any(x => x.Has<StatelessWorkerAttribute>());

            bool hasNonDefaultPlacement = blend.Flavors
                .Any(x => x.Has<PlacementAttribute>());

            if (!isStatelessWorker || !hasNonDefaultPlacement)
                return null;

            return "Specifying placement together with StatelessWorker doesn't make sense";
        }
    }
}
