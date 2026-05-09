using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerPassiveAutoAttackRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<PassiveAutoAttackRequestEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<PassiveAutoAttackRequestEvent>>();
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

        private static void Handle(in NetworkEventFromClient<PassiveAutoAttackRequestEvent> request)
        {
            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player))
                return;

            ServerReceiveCombatCommandsSystem.CreateRequest(
                player.GID,
                CombatAbilityId.BasicMeleeAuto,
                request.Value.Target,
                request.Value.ShotSequence);
        }
    }
}
