using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public struct PlacementPreview : IComponent, ITrackableChanged
    {
        public ushort BuildingId;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsValid;
        public PlacementInvalidReason InvalidReason;
    }
}
