using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EncounterDirectorConfig : IResource
    {
        public readonly float CellRadius;
        public readonly float BaseThreatPerSecond;
        public readonly float NoiseThreatMultiplier;
        public readonly float LootThreatMultiplier;
        public readonly float MinReliefSeconds;
        public readonly float MinCooldownSeconds;
        public readonly int MaxAliveEnemiesPerCell;
        public readonly float BuildUpThreshold;
        public readonly float PeakThreshold;

        public EncounterDirectorConfig(
            float cellRadius,
            float baseThreatPerSecond,
            float noiseThreatMultiplier,
            float lootThreatMultiplier,
            float minReliefSeconds,
            float minCooldownSeconds,
            int maxAliveEnemiesPerCell,
            float buildUpThreshold,
            float peakThreshold)
        {
            CellRadius = cellRadius;
            BaseThreatPerSecond = baseThreatPerSecond;
            NoiseThreatMultiplier = noiseThreatMultiplier;
            LootThreatMultiplier = lootThreatMultiplier;
            MinReliefSeconds = minReliefSeconds;
            MinCooldownSeconds = minCooldownSeconds;
            MaxAliveEnemiesPerCell = maxAliveEnemiesPerCell;
            BuildUpThreshold = buildUpThreshold;
            PeakThreshold = peakThreshold;
        }

        public static EncounterDirectorConfig CreateDefault()
        {
            return new EncounterDirectorConfig(
                cellRadius: 35f,
                baseThreatPerSecond: 2.5f,
                noiseThreatMultiplier: 1.25f,
                lootThreatMultiplier: 1.5f,
                minReliefSeconds: 20f,
                minCooldownSeconds: 15f,
                maxAliveEnemiesPerCell: 24,
                buildUpThreshold: 30f,
                peakThreshold: 75f);
        }
    }
}
