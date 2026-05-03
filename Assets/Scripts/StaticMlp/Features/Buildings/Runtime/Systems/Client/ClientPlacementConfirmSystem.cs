using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
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

            if (NetworkRuntime.LocalPeerId.Value == 0)
                return;

            if (!PlacementPreviewEntityUtility.TryGet(out var previewEntity)
                || !previewEntity.Has<PlacementPreview>())
                return;

            var preview = previewEntity.Read<PlacementPreview>();
            if (!preview.IsValid)
                return;

            ref var outbox = ref CW.GetResource<NetOutbox>();
            var request = new PlaceBuildingRequestEvent(
                preview.BuildingId,
                preview.Position,
                preview.Rotation);

            outbox.EnqueueNetworkEvent(
                new NetworkPeerId(0),
                GameplayEventTypeIds.PlaceBuildingRequest,
                ConstructionEventCodec.Write(request),
                NetDelivery.ReliableSequenced);

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
