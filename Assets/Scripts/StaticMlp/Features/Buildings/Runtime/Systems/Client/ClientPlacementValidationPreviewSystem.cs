using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientPlacementValidationPreviewSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in CW.Query<All<PlacementPreview, PlacementPreviewViewState>>().Entities())
            {
                ref var preview = ref e.Mut<PlacementPreview>();
                var id = new BuildingId(preview.BuildingId);

                var validation = StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(id, out var definition)
                    ? ConstructionPlacementValidator.ValidateClient(definition, preview.Position, preview.Rotation)
                    : PlacementValidationResult.Invalid(PlacementInvalidReason.UnknownBuilding);

                preview.IsValid = validation.IsValid;
                preview.InvalidReason = validation.Reason;

                var viewState = new PlacementPreviewViewState
                {
                    IsValid = validation.IsValid,
                    InvalidReason = validation.Reason
                };

                ref var existing = ref e.Mut<PlacementPreviewViewState>();
                existing = viewState;
            }
        }
    }
}
