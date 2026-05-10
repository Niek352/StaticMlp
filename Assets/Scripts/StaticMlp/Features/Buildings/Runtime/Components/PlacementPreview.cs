using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public struct PlacementPreview : IComponent, ITrackableChanged
    {
        public BuildingId BuildingId;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsValid;
        public PlacementInvalidReason InvalidReason;
    }
}
