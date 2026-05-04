using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct PlaceBuildingRequestEvent : IEvent, IEventConfig<PlaceBuildingRequestEvent>
    {
        public ushort BuildingId;
        public Vector3 Position;
        public Quaternion Rotation;

        public PlaceBuildingRequestEvent(ushort buildingId, Vector3 position, Quaternion rotation)
        {
            BuildingId = buildingId;
            Position = position;
            Rotation = rotation;
        }

        public EventTypeConfig<PlaceBuildingRequestEvent> Config() =>
            new(guid: new Guid("b877f5b3-e768-4e7c-8c5a-816ced8e01f2"));
    }
}
