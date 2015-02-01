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
            new StatelessWorkerCannotSpecifyPlacement(),
            new ReentrantCannotBeMixedWithAlwaysInterleave()
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
                    string.Format("Type {0} has invalid mix of attributes: {1}", type, errors.First()));
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

            return "Specifying placement attributes together with StatelessWorker doesn't make any sense";
        }
    }

    public class ReentrantCannotBeMixedWithAlwaysInterleave : Recipe
    {
        protected override string Validate(Blend blend)
        {
            bool isReentrant = blend.Flavors
                .Any(x => x.Has<ReentrantAttribute>());

            bool hasInterleaveSpecified = blend.Flavors
                .Any(x => x.Has<AlwaysInterleaveAttribute>());
            
            if  (!isReentrant || !hasInterleaveSpecified)
                return null;

            return "Either specify Reentrant or mark individual handlers with AlwaysInterleave";
        }
    }
}
