using System;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingInteractionHandlerRegistry
    {
        private static readonly IBuildingInteractionHandler[] Handlers =
        {
            new DepositConstructionResourcesInteractionHandler(),
            new ContributeBuildWorkInteractionHandler(),
            new DepositCarriedResourcesToStockpileInteractionHandler(),
            new OpenOperationIntentInteractionHandler()
        };

        public static IBuildingInteractionHandler Get(BuildingInteractionKind kind)
        {
            for (var i = 0; i < Handlers.Length; i++)
            {
                if (Handlers[i].CanHandle(kind))
                    return Handlers[i];
            }

            throw new InvalidOperationException($"Unsupported building interaction {kind}.");
        }
    }
}
