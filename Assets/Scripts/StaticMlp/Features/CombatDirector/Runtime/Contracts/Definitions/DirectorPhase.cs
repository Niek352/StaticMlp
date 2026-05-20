namespace StaticMlp.Features.CombatDirector
{
    public enum DirectorPhase : byte
    {
        Dormant = 0,
        Ambient = 1,
        Contact = 2,
        Suspicion = 3,
        Escalation = 4,
        PressureEvent = 5,
        Recovery = 6,
        Cooldown = 7
    }
}
