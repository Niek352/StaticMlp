using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.Combat
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
