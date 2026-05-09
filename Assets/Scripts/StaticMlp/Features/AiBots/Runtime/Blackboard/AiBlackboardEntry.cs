using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public struct AiBlackboardEntry : IMultiComponent
    {
        public ushort VariableId;
        public AiBlackboardValueKind Kind;
        public float FloatValue;
        public EntityGID EntityValue;
        public Vector3 VectorValue;
    }
}
