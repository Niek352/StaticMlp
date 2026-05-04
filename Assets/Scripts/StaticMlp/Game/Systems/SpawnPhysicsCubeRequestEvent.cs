using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Game.Systems
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct SpawnPhysicsCubeRequestEvent : IEvent, IEventConfig<SpawnPhysicsCubeRequestEvent>
    {
        public float CameraYaw;

        public SpawnPhysicsCubeRequestEvent(float cameraYaw)
        {
            CameraYaw = cameraYaw;
        }

        public EventTypeConfig<SpawnPhysicsCubeRequestEvent> Config() =>
            new(guid: new Guid("7f3b4b29-2bd0-4bc5-b548-2e09e302db47"));
    }
}
