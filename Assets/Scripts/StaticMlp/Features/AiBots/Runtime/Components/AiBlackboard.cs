using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public struct AiBlackboard : IComponent
    {
        public float Hunger;
        public float Health01;
        public float Fear;
        public float EnemyDistance;
        public float WoodStorage01;
        public EntityGID Enemy;
        public EntityGID Leader;
        public EntityGID Home;
        public EntityGID WorkTarget;
        public Vector3 LastKnownEnemyPosition;
    }
}
