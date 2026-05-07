using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public struct AiMoveRequest : IComponent
    {
        public Vector3 Destination;
        public float StopDistance;
    }
}
