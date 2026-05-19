using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.CombatDirector
{
    public struct SpawnSourceTelegraphState : IComponent
    {
        public SpawnSourceType SourceType;
        public Vector3 Position;
        public float Radius;
    }
}
