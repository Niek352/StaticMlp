using System;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    [Serializable]
    public struct InitialConstructionSiteDefinition
    {
        public ushort BuildingId;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool StartReadyToBuild;
        public float InitialBuildWork;
    }
}
