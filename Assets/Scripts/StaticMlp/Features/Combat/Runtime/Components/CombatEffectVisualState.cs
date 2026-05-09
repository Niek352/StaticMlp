using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public struct CombatEffectVisualState : IViewComponent
    {
        public CombatEffectVisualType EffectType;
        public Vector3 Position;
        public float Radius;
        public float RemainingLifetime;
        public float TotalLifetime;
    }
}
