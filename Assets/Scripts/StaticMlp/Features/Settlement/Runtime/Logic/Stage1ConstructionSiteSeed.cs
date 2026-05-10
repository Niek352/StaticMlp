using System;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    [Serializable]
    public struct Stage1ConstructionSiteSeed
    {
        public ushort BuildingId;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool StartReadyToBuild;
        public float InitialBuildWork;
    }
}
