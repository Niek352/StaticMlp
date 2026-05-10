using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Statuses
{
    public struct AreaEffectState : IComponent
    {
        public Vector3 Position;
        public float Radius;
        public float TickInterval;
        public float TickTimer;
        public float DamagePerTick;
        public EntityGID Source;
        public uint RequestId;
        public uint RootEffectId;
        public byte ChainDepth;
        public byte MaxDepth;
        public DamageType DamageType;
    }
}
