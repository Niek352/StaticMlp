using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public struct CellAliveEnemyCaps : IComponent
    {
        public int AmbientMinAliveEnemies;
        public int AmbientMaxAliveEnemies;
        public int EncounterMinAliveEnemies;
        public int EncounterMaxAliveEnemies;
        public int EscalationMinAliveEnemies;
        public int EscalationMaxAliveEnemies;
        public int AmbientAliveEnemies;
        public int EncounterAliveEnemies;
    }
}
