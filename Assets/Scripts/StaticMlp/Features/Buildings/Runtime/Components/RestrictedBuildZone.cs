using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public struct RestrictedBuildZone : IComponent
    {
        public Vector3 Center;
        public float Radius;
    }
}
