using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Npc
{
    public sealed class NpcRosterRecordFactory : NetEntityFactory<NpcRosterRecordNetworkEntity>, IResource
    {
        public EntityGID Spawn(in NpcRosterRecordSpawnSpec spec)
        {
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                NpcGameplayFeature.NPC_ROSTER_RECORD);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in NpcRosterRecordSpawnSpec spec)
        {
            entity.Set<NpcRosterRecordTag>();
            entity.Set(new NpcRosterRecord
            {
                DefinitionId = spec.DefinitionId,
                Class = spec.Class,
                AcquisitionPath = spec.AcquisitionPath,
                State = spec.State,
                CreatedServerTick = spec.CreatedServerTick
            });
        }
    }
}
