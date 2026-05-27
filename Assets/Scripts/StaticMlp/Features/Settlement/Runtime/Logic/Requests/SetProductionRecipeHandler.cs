using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class SetProductionRecipeHandler
        : IRequestHandler<SetProductionRecipeRequestEvent, SetProductionRecipeResultEvent>
    {
        private readonly float _interactionRange;

        public SetProductionRecipeHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public SetProductionRecipeResultEvent Handle(
            NetworkPeerId sourcePeer,
            in SetProductionRecipeRequestEvent request)
        {
            var rejected = new SetProductionRecipeResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Building = request.Building,
                ActiveRecipeId = request.RecipeId
            };

            if (sourcePeer.Value == 0
                || !request.Building.TryUnpack<ServerWT>(out var building)
                || !building.Has<FinishedBuildingTag>()
                || !building.Has<ConstructionSiteState>()
                || !building.Has<ConstructionTransform>()
                || !building.Has<ProductionStationOperationState>())
            {
                return rejected;
            }

            ref readonly var site = ref building.Read<ConstructionSiteState>();
            if (site.Phase != ConstructionPhase.Completed)
                return rejected;

            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.OpensQueue) == 0)
                return rejected;

            ref readonly var transform = ref building.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            ref readonly var station = ref building.Read<ProductionStationOperationState>();
            var requestedRecipeId = new ProductionRecipeId(request.RecipeId);
            if (!TryGetStationRecipe(station.Station, requestedRecipeId, out var requestedRecipe))
                return rejected;

            var unlockState = SW.GetResource<SettlementUnlockState>();
            if (!UnlockEvaluation.IsMet(in requestedRecipe.UnlockRequirement, unlockState))
                return rejected;

            if (station.ActiveRecipe == requestedRecipeId)
            {
                return new SetProductionRecipeResultEvent
                {
                    RequestId = request.RequestId,
                    Status = RequestStatus.Accepted,
                    Building = request.Building,
                    ActiveRecipeId = station.ActiveRecipeId
                };
            }

            if (station.WorkDone < 0f)
                throw new InvalidOperationException(
                    $"Production station {building.GID.Raw} has negative work progress {station.WorkDone}.");

            if (station.WorkDone > 0f || HasIncompatibleBufferedInputs(building, in requestedRecipe))
                return rejected;

            ref var mutableStation = ref ReplicationMut.Mut<ProductionStationOperationState>(building);
            mutableStation.ActiveRecipeId = request.RecipeId;
            mutableStation.BlockedReasonValue = (byte)ProductionStationBlockedReason.None;

            return new SetProductionRecipeResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Building = request.Building,
                ActiveRecipeId = mutableStation.ActiveRecipeId
            };
        }

        private static bool TryGetStationRecipe(
            ProductionStationId stationId,
            ProductionRecipeId recipeId,
            out ProductionRecipeDefinition definition)
        {
            for (var i = 0; i < ProductionRecipeCatalog.All.Count; i++)
            {
                var candidate = ProductionRecipeCatalog.All[i];
                if (candidate.StationId != stationId || candidate.Id != recipeId)
                    continue;

                definition = candidate;
                return true;
            }

            definition = default;
            return false;
        }

        private static bool HasIncompatibleBufferedInputs(
            SW.Entity building,
            in ProductionRecipeDefinition requestedRecipe)
        {
            ref readonly var rows = ref building.Ref<SW.Multi<ProductionStationInputResource>>();
            for (var i = 0; i < rows.Length; i++)
            {
                ref readonly var row = ref rows[i];
                if (row.Amount < 0)
                    throw new InvalidOperationException(
                        $"Production station input resource id {row.Id.Value} has negative amount {row.Amount}.");

                if (row.Amount == 0)
                    continue;

                if (!RecipeUsesInput(in requestedRecipe, row.Id))
                    return true;
            }

            return false;
        }

        private static bool RecipeUsesInput(in ProductionRecipeDefinition recipe, ResourceId resourceId)
        {
            for (var i = 0; i < recipe.Inputs.Length; i++)
            {
                if (recipe.Inputs[i].Id == resourceId)
                    return true;
            }

            return recipe.FuelRequirement.HasValue && recipe.FuelRequirement.Value.Id == resourceId;
        }
    }
}
