using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientPlacementConfirmSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Primary))
                return;

            ref var menuState = ref CW.GetResource<BuildingMenuState>();
            if (menuState.SelectionFrame == Time.frameCount)
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

            if (!CW.SendToServerEvent(in request))
                return;

            menuState.ClearSelection();

            PlacementPreviewEntityUtility.DestroyAll();
        }
    }
}
