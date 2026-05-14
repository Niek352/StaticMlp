using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeFactory : NetEntityFactory<OpenWorldResourceNodeNetworkEntity>, IResource
    {
        public EntityGID Spawn(in OpenWorldResourceNodeSpawnSpec spec)
        {
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                OpenWorldResourceNetworkArchetypeIds.ResourceNode);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in OpenWorldResourceNodeSpawnSpec spec)
        {
            entity.Set<OpenWorldResourceNodeTag>();
            entity.Set(new OpenWorldResourceNodeState
            {
                PlacementId = spec.PlacementId,
                KindIdValue = spec.KindId.Value,
                RemainingAmount = spec.RemainingAmount
            });
            entity.Set(new OpenWorldResourceNodeTransform
            {
                Position = spec.Position,
                YawDegrees = spec.YawDegrees,
                Scale = spec.Scale
            });
            entity.Set(new OpenWorldChunkRef
            {
                X = spec.ChunkId.X,
                Z = spec.ChunkId.Z
            });
        }
    }
}
