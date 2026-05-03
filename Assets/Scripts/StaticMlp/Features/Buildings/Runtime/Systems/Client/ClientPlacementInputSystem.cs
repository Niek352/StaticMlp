using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Game.Systems.Client;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
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
            if (!BuildingMenuStateUtility.TryGet(out var menuEntity, out var menuState)
                || !menuState.HasSelection)
            {
                PlacementPreviewEntityUtility.DestroyAll();
                return;
            }

            if (BuildingPlacementInput.CancelPlacementWasPressedProvider())
            {
                ClearSelection(menuEntity);
                PlacementPreviewEntityUtility.DestroyAll();
                return;
            }

            var id = new BuildingId(menuState.SelectedBuildingId);
            if (!StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(id, out var definition))
            {
                ClearSelection(menuEntity);
                PlacementPreviewEntityUtility.DestroyAll();
                return;
            }

            var previewEntity = PlacementPreviewEntityUtility.GetOrCreate(definition);
            var preview = previewEntity.Read<PlacementPreview>();

            if (BuildingPlacementInput.RotatePlacementWasPressedProvider())
                preview.Rotation = Quaternion.Euler(0f, preview.Rotation.eulerAngles.y + 90f, 0f);

            preview.Position = ReadPlacementPosition();
            preview.BuildingId = menuState.SelectedBuildingId;

            ref var previewRef = ref previewEntity.Mut<PlacementPreview>();
            previewRef = preview;

            ref var viewTransform = ref previewEntity.Mut<ViewTransform>();
            viewTransform.RenderPosition = preview.Position;
            viewTransform.RenderRotation = preview.Rotation;
        }

        private Vector3 ReadPlacementPosition()
        {
            if (BuildingPlacementInput.AimRayProvider(out var ray))
            {
                if (Physics.Raycast(ray, out var hit, 500f))
                    return hit.point;

                var groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out var enter))
                    return ray.GetPoint(enter);
            }

            if (TryGetLocalPlayerPosition(out var playerPosition))
            {
                var cameraYaw = Quaternion.Euler(0f, NetworkInput.CameraYawProvider(), 0f);
                return playerPosition + cameraYaw * Vector3.forward * _fallbackPlacementDistance;
            }

            return Vector3.zero;
        }

        private static bool TryGetLocalPlayerPosition(out Vector3 position)
        {
            foreach (var e in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = e.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }

        private static void ClearSelection(CW.Entity menuEntity)
        {
            ref var state = ref menuEntity.Mut<BuildingMenuState>();
            state.ClearSelection();
            BuildingMenuRuntime.Publish(in state);
        }
    }
}
