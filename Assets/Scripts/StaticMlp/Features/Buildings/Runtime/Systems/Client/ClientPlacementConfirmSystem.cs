using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientPlacementConfirmSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Primary))
                return;

            if (EventSystem.current == null)
                throw new MissingReferenceException($"{nameof(ClientPlacementConfirmSystem)} requires an active {nameof(EventSystem)}.");

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            ref var menuState = ref CW.GetResource<BuildingMenuState>();
            if (!PlacementPreviewEntityUtility.TryGet(out var previewEntity)
                || !previewEntity.Has<PlacementPreview>())
                return;

            var preview = previewEntity.Read<PlacementPreview>();
            if (!preview.IsValid)
                return;

            var request = new PlaceBuildingRequestEvent(
                preview.BuildingId.Value,
                preview.Position,
                preview.Rotation);

            CW.SendToServer(in request);
            menuState.ClearSelection();

            PlacementPreviewEntityUtility.DestroyAll();
        }
    }
}
