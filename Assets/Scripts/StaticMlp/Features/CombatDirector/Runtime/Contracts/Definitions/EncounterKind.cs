namespace StaticMlp.Features.CombatDirector
{
    public enum EncounterKind : byte
    {
        None = 0,
        AmbientSolo = 1,
        AmbientSmallPack = 2,
        CampContact = 3,
        LairContact = 4,
        PatrolContact = 5,
        ResourceGuard = 6,
        CaravanAmbush = 7,
        BaseRaid = 8,
        BossEvent = 9
    }
}
