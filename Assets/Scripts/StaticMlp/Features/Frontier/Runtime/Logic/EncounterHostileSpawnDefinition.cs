using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public readonly struct EncounterHostileSpawnDefinition
    {
        public readonly Vector3 Offset;
        public readonly ushort BehaviorId;
        public readonly float Health01;
        public readonly float Hunger;
        public readonly float Fear;

        public EncounterHostileSpawnDefinition(
            Vector3 offset,
            ushort behaviorId,
            float health01,
            float hunger,
            float fear)
        {
            Offset = offset;
            BehaviorId = behaviorId;
            Health01 = health01;
            Hunger = hunger;
            Fear = fear;
        }
    }
}
