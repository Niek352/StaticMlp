using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Player;
using StaticMlp.Game.Input;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientPlacementInputSystem : ISystem
    {
        private readonly float _fallbackPlacementDistance;

        public ClientPlacementInputSystem(float fallbackPlacementDistance = 4f)
        {
            _fallbackPlacementDistance = fallbackPlacementDistance;
        }

        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            ref var menuState = ref CW.GetResource<BuildingMenuState>();
            if (!menuState.HasSelection)
            {
                PlacementPreviewEntityUtility.DestroyAll();
                return;
            }

            if (inputState.WasPressed(CoreInputActions.Cancel)
                || inputState.WasPressed(CoreInputActions.Secondary))
            {
                ClearSelection();
                PlacementPreviewEntityUtility.DestroyAll();
                return;
            }

            var definition = StaticMlp.Features.BuildingCatalog.BuildingCatalogData.Get(menuState.SelectedBuildingId);

            var previewEntity = PlacementPreviewEntityUtility.GetOrCreate(definition);
            var preview = previewEntity.Read<PlacementPreview>();

            if (inputState.WasPressed(BuildingsInputActions.PlacementRotate))
                preview.Rotation = Quaternion.Euler(0f, preview.Rotation.eulerAngles.y + 90f, 0f);

            preview.Position = ReadPlacementPosition(inputState);
            preview.BuildingId = menuState.SelectedBuildingId;

            ref var previewRef = ref previewEntity.Mut<PlacementPreview>();
            previewRef = preview;

            ref var viewTransform = ref previewEntity.Mut<ViewTransform>();
            viewTransform.RenderPosition = preview.Position;
            viewTransform.RenderRotation = preview.Rotation;
        }

        private Vector3 ReadPlacementPosition(ClientInputState inputState)
        {
            if (inputState.TryGetAimRay(out var ray))
            {
                if (Physics.Raycast(ray, out var hit, 500f))
                    return hit.point;

                var groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out var enter))
                    return ray.GetPoint(enter);
            }

            if (ClientLocalPlayer.TryGetPosition(out var playerPosition))
            {
                var cameraState = CW.GetResource<ClientCameraState>();
                var cameraYaw = Quaternion.Euler(0f, cameraState.Yaw, 0f);
                return playerPosition + cameraYaw * Vector3.forward * _fallbackPlacementDistance;
            }

            return Vector3.zero;
        }

        private static void ClearSelection()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.ClearSelection();
        }
    }
}
