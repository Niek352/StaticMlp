using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public readonly struct PlaceBuildingRequestEvent
    {
        public readonly ushort BuildingId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlaceBuildingRequestEvent(ushort buildingId, Vector3 position, Quaternion rotation)
        {
            BuildingId = buildingId;
            Position = position;
            Rotation = rotation;
        }
    }
}
