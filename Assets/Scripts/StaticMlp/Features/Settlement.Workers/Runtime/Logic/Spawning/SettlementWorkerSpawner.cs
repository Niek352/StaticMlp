using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerSpawner
    {
        public static EntityGID Spawn(in SettlementWorkerSpawnSpec spec)
        {
            var localSpec = spec;
            return NetworkEntitySpawner.SpawnServerEntity<AiBotNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                spec.NetworkArchetypeId,
                entity => InitializeWorker(entity, in localSpec));
        }

        private static void InitializeWorker(SW.Entity entity, in SettlementWorkerSpawnSpec spec)
        {
            entity.Set<AiAgentTag>();
            entity.Set<SettlementWorkerTag>();
            entity.Set(new SettlementWorkerIdentity
            {
                HomeAnchorId = spec.HomeAnchorId.Value,
                RoleId = spec.RoleId.Value
            });
            entity.Set(new SettlementWorkerAssignment
            {
                Status = SettlementWorkerAssignmentStatus.Unassigned,
                AnchorId = 0
            });
            AiBotSpawns.ApplyServerAiAgentState(entity, new AiAgentSpawnStateSpec(
                spec.Position,
                spec.Rotation,
                spec.BehaviorId,
                spec.MaxHealth,
                health01: 1f,
                hunger: 0f,
                fear: 0f,
                leader: default));
        }
    }
}
