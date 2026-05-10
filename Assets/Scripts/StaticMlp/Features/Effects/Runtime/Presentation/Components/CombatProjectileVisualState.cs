using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Combat;
using UnityEngine;

namespace StaticMlp.Features.Effects
{
    public struct CombatProjectileVisualState : IViewComponent
    {
        public CombatAbilityId AbilityId;
        public Vector3 Start;
        public Vector3 End;
        public float RemainingLifetime;
        public float TotalLifetime;
        public CombatEffectVisualType ImpactEffectType;
        public Vector3 ImpactPosition;
        public float ImpactRadius;
        public float ImpactLifetime;
    }
}
