using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerReceiveCombatCommandsSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<UseAbilityCommand>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<UseAbilityCommand>>();
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

                CreateRequest(player.GID, evt.Value.Value.AbilityId, evt.Value.Value.Target, evt.Value.Value.ClientCommandId);
            }
        }

        public static SW.Entity CreateRequest(
            EntityGID source,
            CombatAbilityId abilityId,
            EntityGID target,
            uint clientCommandId)
        {
            var request = SW.NewEntity<Default>();
            request.Set(new CombatAbilityRequest
            {
                AbilityId = abilityId,
                Source = source,
                Target = target,
                ClientCommandId = clientCommandId,
            });
            return request;
        }
    }
}
