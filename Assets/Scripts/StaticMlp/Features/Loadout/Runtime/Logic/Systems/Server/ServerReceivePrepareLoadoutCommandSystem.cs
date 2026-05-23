using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Loadout
{
    public sealed class ServerReceivePrepareLoadoutCommandSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<PrepareLoadoutCommand>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<PrepareLoadoutCommand>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                if (IsBossBuildCommitted())
                    continue;

                if (!ServerPeerPlayers.TryGetPlayer(evt.Value.SourcePeer, out var player))
                    continue;

                player.Set(new OwnerLoadoutSelection
                {
                    PrimaryModuleId = evt.Value.Value.PrimaryModuleId
                });
                SW.SendEvent(new CampFlowLoadoutPreparedEvent(evt.Value.Value.AnchorId));
            }
        }

        private static bool IsBossBuildCommitted()
        {
            foreach (var anchor in SW.Query<All<BossLoadoutPreparationState>>().Entities())
            {
                if (anchor.Read<BossLoadoutPreparationState>().Status == BossLoadoutPreparationStatus.Committed)
                    return true;
            }

            return false;
        }
    }
}
