using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Progression
{
    public sealed class ServerStage1BossPreparationProgressionSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<PrepareBossRequestEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<PrepareBossRequestEvent>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
                Handle(in evt.Value);
        }

        private static void Handle(in NetworkEventFromClient<PrepareBossRequestEvent> request)
        {
            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player)
                || !player.Has<PreparedBuildSnapshot>())
            {
                return;
            }

            var anchor = Stage1SettlementProgressionQuery.GetServerAnchor(request.Value.AnchorId);
            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            if (!progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                || !progression.TrySpendBossPreparationToken())
            {
                return;
            }

            ref var buildState = ref ReplicationMut.Mut<BossBuildPreparationState>(anchor);
            if (buildState.Status == BossBuildPreparationStatus.Committed)
            {
                progression.GrantBossPreparationTokens(1);
                return;
            }

            ref var committedSnapshot = ref ReplicationMut.Mut<BossPreparedBuildSnapshot>(anchor);
            committedSnapshot.Apply(player.Read<PreparedBuildSnapshot>());
            buildState.Status = BossBuildPreparationStatus.Committed;

            if (progression.HasFlag(ProgressFlagCatalog.BossUnlockedId))
                return;

            progression.ApplyFlag(ProgressFlagCatalog.BossUnlockedId);
            SW.SendEvent(new ProgressFlagAppliedEvent(request.Value.AnchorId, ProgressFlagCatalog.BossUnlockedId));
        }
    }
}
