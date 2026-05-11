using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Build
{
    public sealed class ServerReceivePrepareBuildCommandSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<PrepareBuildCommand>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<PrepareBuildCommand>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                if (!ServerPeerPlayers.TryGetPlayer(evt.Value.SourcePeer, out var player))
                    continue;

                player.Set(new OwnerBuildSelection
                {
                    PrimaryModuleId = evt.Value.Value.PrimaryModuleId
                });
            }
        }
    }
}
