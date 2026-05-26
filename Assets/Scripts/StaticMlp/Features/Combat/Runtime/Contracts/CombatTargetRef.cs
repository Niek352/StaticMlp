using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct CombatTargetRef
    {
        public CombatTargetKind Kind;
        public EntityGID Entity;
        public long PlacementId;
        public int HitPointXQ;
        public int HitPointYQ;
        public int HitPointZQ;
    }
}
