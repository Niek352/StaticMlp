using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Effects
{
    public sealed class ClientCombatVisualSpawnSystem : ISystem
    {
        private const string PROJECTILE_VIEW_PATH = "Views/Combat/CombatProjectileView";
        private const string EFFECT_VIEW_PATH = "Views/Combat/CombatEffectView";

        public void Update()
        {
            var config = CW.GetResource<StatusesConfig>();
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState, PassiveAutoAttackIntent>>().Entities())
                TrySpawnVisuals(player, config);
        }

        private static void TrySpawnVisuals(CW.Entity player, StatusesConfig config)
        {
            ref readonly var intent = ref player.Read<PassiveAutoAttackIntent>();
            if (!player.Has<LocalCombatPredictionState>())
                return;

            ref var prediction = ref player.Mut<LocalCombatPredictionState>();
            if (intent.ShotSequence <= prediction.LastVisualizedCommandId)
                return;

            prediction.LastVisualizedCommandId = intent.ShotSequence;
            if (!intent.Target.TryUnpack<ClientCoreWT>(out var target) || !target.Has<CharacterNetState>())
                return;

            var start = player.Read<CharacterNetState>().Position + Vector3.up * 1.1f;
            var end = target.Read<CharacterNetState>().Position + Vector3.up * 1.1f;

            switch (intent.AbilityId)
            {
                case CombatAbilityId.PoisonArrow:
                    SpawnProjectile(start, end, intent.AbilityId, 0.22f, CombatEffectVisualType.PoisonBurst, target.Read<CharacterNetState>().Position, 0.8f, 0.35f);
                    break;
                case CombatAbilityId.FireFlask:
                    SpawnProjectile(start, end, intent.AbilityId, 0.35f, CombatEffectVisualType.BurningPool, target.Read<CharacterNetState>().Position, config.BurningPoolRadius, config.BurningPoolDuration);
                    break;
            }
        }

        private static void SpawnProjectile(
            Vector3 start,
            Vector3 end,
            CombatAbilityId abilityId,
            float lifetime,
            CombatEffectVisualType impactEffectType,
            Vector3 impactPosition,
            float impactRadius,
            float impactLifetime)
        {
            var entity = ClientOnlyEntities.New();
            entity.Set(new ViewPath(PROJECTILE_VIEW_PATH));
            entity.Set(new CombatProjectileVisualState
            {
                AbilityId = abilityId,
                Start = start,
                End = end,
                RemainingLifetime = lifetime,
                TotalLifetime = lifetime,
                ImpactEffectType = impactEffectType,
                ImpactPosition = impactPosition,
                ImpactRadius = impactRadius,
                ImpactLifetime = impactLifetime,
            });
        }

        public static void SpawnEffect(CombatEffectVisualType effectType, Vector3 position, float radius, float lifetime)
        {
            var entity = ClientOnlyEntities.New();
            entity.Set(new ViewPath(EFFECT_VIEW_PATH));
            entity.Set(new CombatEffectVisualState
            {
                EffectType = effectType,
                Position = position,
                Radius = radius,
                RemainingLifetime = lifetime,
                TotalLifetime = lifetime,
            });
        }
    }
}
