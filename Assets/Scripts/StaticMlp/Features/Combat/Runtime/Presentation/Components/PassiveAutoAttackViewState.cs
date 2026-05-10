using StaticMlp.Features.EcsViews;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public struct PassiveAutoAttackViewState : IViewComponent
    {
        public bool HasTracer;
        public Vector3 TracerStart;
        public Vector3 TracerEnd;
        public float RemainingLifetime;
        public uint ShotSequence;
    }
}
