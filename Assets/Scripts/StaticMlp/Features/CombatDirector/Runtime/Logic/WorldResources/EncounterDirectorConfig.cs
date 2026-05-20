using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class EncounterDirectorConfig : IResource
    {
        public readonly float CellRadius;
        public readonly float NoiseAttentionMultiplier;
        public readonly float CombatAttentionMultiplier;
        public readonly float LootAttentionMultiplier;
        public readonly float TrespassAttentionMultiplier;
        public readonly float FactionAlarmAttentionMultiplier;
        public readonly float AttentionDecayPerSecond;
        public readonly float MinReliefSeconds;
        public readonly float MinCooldownSeconds;
        public readonly int MaxAliveEnemiesPerCell;
        public readonly float MinSpawnSourceDistance;
        public readonly float MaxSpawnSourceDistance;
        public readonly float BuildUpThreshold;
        public readonly float PeakThreshold;

        public EncounterDirectorConfig(
            float cellRadius,
            float noiseAttentionMultiplier,
            float combatAttentionMultiplier,
            float lootAttentionMultiplier,
            float trespassAttentionMultiplier,
            float factionAlarmAttentionMultiplier,
            float attentionDecayPerSecond,
            float minReliefSeconds,
            float minCooldownSeconds,
            int maxAliveEnemiesPerCell,
            float minSpawnSourceDistance,
            float maxSpawnSourceDistance,
            float buildUpThreshold,
            float peakThreshold)
        {
            CellRadius = cellRadius;
            NoiseAttentionMultiplier = noiseAttentionMultiplier;
            CombatAttentionMultiplier = combatAttentionMultiplier;
            LootAttentionMultiplier = lootAttentionMultiplier;
            TrespassAttentionMultiplier = trespassAttentionMultiplier;
            FactionAlarmAttentionMultiplier = factionAlarmAttentionMultiplier;
            AttentionDecayPerSecond = attentionDecayPerSecond;
            MinReliefSeconds = minReliefSeconds;
            MinCooldownSeconds = minCooldownSeconds;
            MaxAliveEnemiesPerCell = maxAliveEnemiesPerCell;
            MinSpawnSourceDistance = minSpawnSourceDistance;
            MaxSpawnSourceDistance = maxSpawnSourceDistance;
            BuildUpThreshold = buildUpThreshold;
            PeakThreshold = peakThreshold;
        }

        public static EncounterDirectorConfig CreateDefault()
        {
            return new EncounterDirectorConfig(
                cellRadius: 35f,
                noiseAttentionMultiplier: 1.25f,
                combatAttentionMultiplier: 3f,
                lootAttentionMultiplier: 1.5f,
                trespassAttentionMultiplier: 2f,
                factionAlarmAttentionMultiplier: 5f,
                attentionDecayPerSecond: 1f,
                minReliefSeconds: 20f,
                minCooldownSeconds: 15f,
                maxAliveEnemiesPerCell: 24,
                minSpawnSourceDistance: 10f,
                maxSpawnSourceDistance: 90f,
                buildUpThreshold: 30f,
                peakThreshold: 75f);
        }
    }
}
