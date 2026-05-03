using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientPlacementConfirmSystem : ISystem
    {
        public void Update()
        {
            if (!BuildingPlacementInput.ConfirmPlacementWasPressedProvider())
                return;

            if (BuildingMenuRuntime.SelectionFrame == Time.frameCount)
                return;

            if (!PlacementPreviewEntityUtility.TryGet(out var previewEntity)
                || !previewEntity.Has<PlacementPreview>())
                return;

            var preview = previewEntity.Read<PlacementPreview>();
            if (!preview.IsValid)
                return;

            var request = new PlaceBuildingRequestEvent(
                preview.BuildingId,
                preview.Position,
                preview.Rotation);

            if (!NetworkEvents.TrySendToServer(in request))
                return;

            if (BuildingMenuStateUtility.TryGet(out var menuEntity, out _))
            {
                ref var state = ref menuEntity.Mut<BuildingMenuState>();
                state.ClearSelection();
                BuildingMenuRuntime.Publish(in state);
            }

            PlacementPreviewEntityUtility.DestroyAll();
        }
    }
}
