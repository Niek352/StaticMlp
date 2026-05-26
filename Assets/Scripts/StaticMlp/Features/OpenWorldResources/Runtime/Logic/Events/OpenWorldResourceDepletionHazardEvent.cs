using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    internal struct OpenWorldResourceDepletionHazardEvent : IEvent
    {
        public EntityGID SourcePlayer;
        public long PlacementId;
        public ResourcePlacementKindId KindId;
        public Vector3 Origin;
    }
}
