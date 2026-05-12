using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class SettlementWorkerFactory : NetEntityFactory<AiBotNetworkEntity>, IResource
    {
        public EntityGID Spawn(in SettlementWorkerSpawnSpec spec)
        {
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                spec.NetworkArchetypeId);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in SettlementWorkerSpawnSpec spec)
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
            SW.GetResource<AiBotFactory>().ApplyServerAiAgentState(entity, new AiAgentSpawnStateSpec(
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
