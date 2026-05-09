using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerPassiveAutoAttackRequestSystem : ISystem
    {
        private readonly System.Func<float> _timeProvider;
        private EventReceiver<ServerWT, NetworkEventFromClient<PassiveAutoAttackRequestEvent>> _requests;

        public ServerPassiveAutoAttackRequestSystem(System.Func<float> timeProvider = null)
        {
            _timeProvider = timeProvider ?? (() => Time.time);
        }

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
            var config = RequireConfig();
            var now = _timeProvider();

            foreach (var evt in _requests)
                Handle(in evt.Value, config, now);
        }

        private static void Handle(
            in NetworkEventFromClient<PassiveAutoAttackRequestEvent> request,
            CombatAutoAttackConfig config,
            float now)
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
            if ((playerPosition - targetPosition).sqrMagnitude > config.Radius * config.Radius)
                return;

            ref var attackState = ref EnsureAttackState(player);
            if (request.Value.ShotSequence <= attackState.LastAcceptedShotSequence)
                return;

            if (now < attackState.NextAttackAt)
                return;

            attackState.LastAcceptedShotSequence = request.Value.ShotSequence;
            attackState.NextAttackAt = now + Mathf.Max(0f, config.FireInterval);

            EffectCommands.CreateDamage(
                player.GID,
                target.GID,
                config.DamageValue,
                DamageType.Physical,
                request.Value.ShotSequence);
        }

        private static ref ServerCombatAttackState EnsureAttackState(SW.Entity attacker)
        {
            if (!attacker.Has<ServerCombatAttackState>())
                attacker.Set(new ServerCombatAttackState());

            return ref attacker.Mut<ServerCombatAttackState>();
        }

        private static CombatAutoAttackConfig RequireConfig()
        {
            if (!SW.HasResource<CombatAutoAttackConfig>())
                throw new System.InvalidOperationException("Combat auto attack config resource is missing.");

            var config = SW.GetResource<CombatAutoAttackConfig>();
            if (config == null)
                throw new System.InvalidOperationException("Combat auto attack config resource is null.");

            return config;
        }
    }
}
