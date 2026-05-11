using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public struct SpawnFrontierEncounterBotEvent : IEvent
    {
        public Vector3 Position;
        public ushort BehaviorId;
        public float Health01;
        public float Hunger;
        public float Fear;
        public ushort AnchorId;
        public FrontierEncounterKind EncounterKind;
        public ushort SourceId;
    }
}
