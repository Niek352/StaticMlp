using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerPassiveAutoAttackRequestSystem : ISystem
    {
        private const float ATTACK_RANGE = 8f;
        private const float DAMAGE_VALUE = 10f;

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
            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player)
                || !player.Has<CharacterNetState>())
                return;

            if (!request.Value.Target.TryUnpack<ServerWT>(out var target)
                || !target.Has<MonsterTag>()
                || !target.Has<CharacterNetState>()
                || !target.Has<Health>())
                return;

            if (target.Read<Health>().Current <= 0f)
                return;

            var playerPosition = player.Read<CharacterNetState>().Position;
            var targetPosition = target.Read<CharacterNetState>().Position;
            if ((playerPosition - targetPosition).sqrMagnitude > ATTACK_RANGE * ATTACK_RANGE)
                return;

            EffectCommands.CreateDamage(
                player.GID,
                target.GID,
                DAMAGE_VALUE,
                DamageType.Physical,
                request.Value.ShotSequence);
        }
    }
}
