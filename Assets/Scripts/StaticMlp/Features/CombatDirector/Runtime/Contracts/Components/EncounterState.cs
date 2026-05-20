using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct EncounterState : IComponent
    {
        public int CellId;
        public int EncounterId;
        public EncounterKind Kind;
        public EncounterIntensity Intensity;
        public float TimeAlive;
        public int AliveEnemyCount;
        public bool EscalationAllowed;
    }
}
