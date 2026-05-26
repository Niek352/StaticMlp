using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackPresentationSystem : ISystem
    {
        private readonly List<EntityGID> _players = new();
        private readonly List<EntityGID> _currentTargets = new();
        private readonly HashSet<ulong> _currentTargetRaws = new();
        private readonly List<ShotSnapshot> _shots = new();

        public void Update()
        {
            var config = CW.GetResource<CombatPresentationConfig>();
            var deltaTime = CW.GetResource<GameTime>().DeltaTime;

            CollectCurrentPlayersAndTargets();
            DecayTracers(deltaTime);
            DecayHighlights(config, deltaTime);
            CollectNewShots(config);
            ApplyShots(config);
            ApplyCurrentTargetHighlights(config);
        }

        private void CollectCurrentPlayersAndTargets()
        {
            _players.Clear();
            _currentTargets.Clear();
            _currentTargetRaws.Clear();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState, PassiveAutoAttackState>>().Entities())
            {
                _players.Add(player.GID);

                var currentTarget = player.Read<PassiveAutoAttackState>().CurrentTarget;
                if (currentTarget.Kind != CombatTargetKind.ActorEntity || currentTarget.Entity.Raw == 0ul)
                    continue;

                _currentTargets.Add(currentTarget.Entity);
                _currentTargetRaws.Add(currentTarget.Entity.Raw);
            }
        }

        private static void DecayTracers(float deltaTime)
        {
            foreach (var entity in CW.Query<All<PassiveAutoAttackViewState>>().Entities())
            {
                ref var state = ref entity.Mut<PassiveAutoAttackViewState>();
                if (!state.HasTracer)
                    continue;

                state.RemainingLifetime -= deltaTime;
                if (state.RemainingLifetime <= 0f)
                {
                    state.RemainingLifetime = 0f;
                    state.HasTracer = false;
                }
            }
        }

        private void DecayHighlights(CombatPresentationConfig config, float deltaTime)
        {
            foreach (var entity in CW.Query<All<PassiveAutoAttackTargetViewState>>().Entities())
            {
                if (_currentTargetRaws.Contains(entity.GID.Raw))
                    continue;

                ref var state = ref entity.Mut<PassiveAutoAttackTargetViewState>();
                state.IsHighlighted = false;
                state.RemainingFade -= deltaTime;
                if (state.RemainingFade <= 0f || config.HighlightFadeOut <= 0f)
                {
                    state.RemainingFade = 0f;
                    state.HighlightIntensity = 0f;
                    continue;
                }

                state.HighlightIntensity = Mathf.Clamp01(state.RemainingFade / config.HighlightFadeOut);
            }
        }

        private void CollectNewShots(CombatPresentationConfig config)
        {
            _shots.Clear();

            for (var i = 0; i < _players.Count; i++)
            {
                if (!_players[i].TryUnpack<ClientCoreWT>(out var player))
                    continue;

                if (!player.Has<PassiveAutoAttackIntent>())
                    continue;

                ref readonly var intent = ref player.Read<PassiveAutoAttackIntent>();
                if (!TryGetTargetPosition(intent.Target, out var targetPosition))
                    continue;

                var viewState = player.Has<PassiveAutoAttackViewState>()
                    ? player.Read<PassiveAutoAttackViewState>()
                    : default;
                if (viewState.ShotSequence >= intent.ShotSequence)
                    continue;

                var shooterPosition = player.Read<CharacterNetState>().Position + Vector3.up;
                _shots.Add(new ShotSnapshot(
                    player.GID,
                    intent.Target,
                    intent.ShotSequence,
                    shooterPosition,
                    targetPosition,
                    config.TracerLifetime));
            }
        }

        private void ApplyShots(CombatPresentationConfig config)
        {
            for (var i = 0; i < _shots.Count; i++)
            {
                var shot = _shots[i];
                if (!shot.Shooter.TryUnpack<ClientCoreWT>(out var shooter))
                    continue;

                shooter.Set(new PassiveAutoAttackViewState
                {
                    HasTracer = true,
                    TracerStart = shot.Start,
                    TracerEnd = shot.End,
                    RemainingLifetime = Mathf.Max(0f, shot.Lifetime),
                    ShotSequence = shot.Sequence
                });

                if (shot.Target.Kind != CombatTargetKind.ActorEntity
                    || !shot.Target.Entity.TryUnpack<ClientCoreWT>(out var target))
                    continue;

                target.Set(new PassiveAutoAttackTargetViewState
                {
                    IsHighlighted = true,
                    HighlightIntensity = 1f,
                    RemainingFade = Mathf.Max(0f, config.HighlightFadeOut)
                });
            }
        }

        private void ApplyCurrentTargetHighlights(CombatPresentationConfig config)
        {
            for (var i = 0; i < _currentTargets.Count; i++)
            {
                if (!_currentTargets[i].TryUnpack<ClientCoreWT>(out var target))
                    continue;

                target.Set(new PassiveAutoAttackTargetViewState
                {
                    IsHighlighted = true,
                    HighlightIntensity = 1f,
                    RemainingFade = Mathf.Max(0f, config.HighlightFadeOut)
                });
            }
        }

        private readonly struct ShotSnapshot
        {
            public readonly EntityGID Shooter;
            public readonly CombatTargetRef Target;
            public readonly uint Sequence;
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly float Lifetime;

            public ShotSnapshot(EntityGID shooter, CombatTargetRef target, uint sequence, Vector3 start, Vector3 end, float lifetime)
            {
                Shooter = shooter;
                Target = target;
                Sequence = sequence;
                Start = start;
                End = end;
                Lifetime = lifetime;
            }
        }

        private static bool TryGetTargetPosition(CombatTargetRef targetRef, out Vector3 position)
        {
            switch (targetRef.Kind)
            {
                case CombatTargetKind.ActorEntity:
                    if (targetRef.Entity.TryUnpack<ClientCoreWT>(out var target) && target.Has<CharacterNetState>())
                    {
                        position = target.Read<CharacterNetState>().Position + Vector3.up;
                        return true;
                    }

                    break;

                case CombatTargetKind.StaticPlacement:
                    if (targetRef.PlacementId > 0L)
                    {
                        position = QuantizedHitPointToWorld(targetRef);
                        return true;
                    }

                    break;
            }

            position = default;
            return false;
        }

        private static Vector3 QuantizedHitPointToWorld(CombatTargetRef targetRef)
        {
            const float quantization = 0.01f;
            return new Vector3(
                targetRef.HitPointXQ * quantization,
                targetRef.HitPointYQ * quantization,
                targetRef.HitPointZQ * quantization);
        }
    }
}
