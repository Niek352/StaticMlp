using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using StaticMlp.Features.CombatDirector;

namespace StaticMlp.Tests.CombatDirector
{
    public sealed class CombatDirectorContractsTests
    {
        [Test]
        public void DirectorPhase_UsesDesignLockOrdering()
        {
            Assert.That((byte)DirectorPhase.Dormant, Is.EqualTo(0));
            Assert.That((byte)DirectorPhase.Ambient, Is.EqualTo(1));
            Assert.That((byte)DirectorPhase.Contact, Is.EqualTo(2));
            Assert.That((byte)DirectorPhase.Suspicion, Is.EqualTo(3));
            Assert.That((byte)DirectorPhase.Escalation, Is.EqualTo(4));
            Assert.That((byte)DirectorPhase.PressureEvent, Is.EqualTo(5));
            Assert.That((byte)DirectorPhase.Recovery, Is.EqualTo(6));
            Assert.That((byte)DirectorPhase.Cooldown, Is.EqualTo(7));
        }

        [Test]
        public void EncounterContracts_UseDesignLockOrdering()
        {
            Assert.That((byte)EncounterKind.None, Is.EqualTo(0));
            Assert.That((byte)EncounterKind.AmbientSolo, Is.EqualTo(1));
            Assert.That((byte)EncounterKind.AmbientSmallPack, Is.EqualTo(2));
            Assert.That((byte)EncounterKind.CampContact, Is.EqualTo(3));
            Assert.That((byte)EncounterKind.LairContact, Is.EqualTo(4));
            Assert.That((byte)EncounterKind.PatrolContact, Is.EqualTo(5));
            Assert.That((byte)EncounterKind.ResourceGuard, Is.EqualTo(6));
            Assert.That((byte)EncounterKind.CaravanAmbush, Is.EqualTo(7));
            Assert.That((byte)EncounterKind.BaseRaid, Is.EqualTo(8));
            Assert.That((byte)EncounterKind.BossEvent, Is.EqualTo(9));

            Assert.That((byte)EncounterIntensity.Passive, Is.EqualTo(0));
            Assert.That((byte)EncounterIntensity.Minor, Is.EqualTo(1));
            Assert.That((byte)EncounterIntensity.Moderate, Is.EqualTo(2));
            Assert.That((byte)EncounterIntensity.Dangerous, Is.EqualTo(3));
            Assert.That((byte)EncounterIntensity.Raid, Is.EqualTo(4));
            Assert.That((byte)EncounterIntensity.Boss, Is.EqualTo(5));
        }

        [Test]
        public void EnemyRole_UsesDesignLockOrdering()
        {
            Assert.That((byte)EnemyRole.Swarmer, Is.EqualTo(1));
            Assert.That((byte)EnemyRole.Marker, Is.EqualTo(2));
            Assert.That((byte)EnemyRole.AnchorElite, Is.EqualTo(3));
        }

        [Test]
        public void SpawnSourceType_UsesDesignLockOrdering()
        {
            Assert.That((byte)SpawnSourceType.Burrow, Is.EqualTo(1));
            Assert.That((byte)SpawnSourceType.Rift, Is.EqualTo(2));
        }

        [Test]
        public void SpawnSourceKind_UsesOpenWorldDesignLockOrdering()
        {
            Assert.That((byte)SpawnSourceKind.AmbientPoint, Is.EqualTo(1));
            Assert.That((byte)SpawnSourceKind.PatrolRoute, Is.EqualTo(2));
            Assert.That((byte)SpawnSourceKind.CampGate, Is.EqualTo(3));
            Assert.That((byte)SpawnSourceKind.LairEntrance, Is.EqualTo(4));
            Assert.That((byte)SpawnSourceKind.Rift, Is.EqualTo(5));
            Assert.That((byte)SpawnSourceKind.RoadAmbush, Is.EqualTo(6));
            Assert.That((byte)SpawnSourceKind.BaseRaidEntry, Is.EqualTo(7));
        }

        [Test]
        public void AmbientSpawnKind_UsesOpenWorldDesignLockOrdering()
        {
            Assert.That((byte)AmbientSpawnKind.None, Is.EqualTo(0));
            Assert.That((byte)AmbientSpawnKind.SoloAnimal, Is.EqualTo(1));
            Assert.That((byte)AmbientSpawnKind.SoloBandit, Is.EqualTo(2));
            Assert.That((byte)AmbientSpawnKind.SmallPack, Is.EqualTo(3));
            Assert.That((byte)AmbientSpawnKind.Patrol, Is.EqualTo(4));
            Assert.That((byte)AmbientSpawnKind.ResourceGuardian, Is.EqualTo(5));
        }

        [Test]
        public void SpawnSource_DefaultInitialization_DoesNotAllowAnyDirectorUsage()
        {
            var source = new SpawnSource();

            Assert.That(source.Kind, Is.EqualTo((SpawnSourceKind)0));
            Assert.That(source.FactionId, Is.EqualTo(0));
            Assert.That(source.BiomeId, Is.EqualTo(0));
            Assert.That(source.AllowsAmbient, Is.False);
            Assert.That(source.AllowsEscalation, Is.False);
            Assert.That(source.AllowsPressureEvent, Is.False);
        }

        [Test]
        public void AmbientContracts_DefaultsExposeCooldownAndConservativeCaps()
        {
            var marker = new AmbientSpawnMarker();
            Assert.That(marker.Kind, Is.EqualTo(AmbientSpawnKind.None));
            Assert.That(marker.CooldownSeconds, Is.EqualTo(0f));
            Assert.That(marker.CooldownRemaining, Is.EqualTo(0f));

            var config = EncounterDirectorConfig.CreateDefault();
            Assert.That(config.AmbientMinAliveEnemiesPerCell, Is.EqualTo(1));
            Assert.That(config.AmbientMaxAliveEnemiesPerCell, Is.EqualTo(4));
            Assert.That(config.EncounterMinAliveEnemiesPerCell, Is.EqualTo(1));
            Assert.That(config.EncounterMaxAliveEnemiesPerCell, Is.EqualTo(6));
            Assert.That(config.EscalationMinAliveEnemiesPerCell, Is.EqualTo(2));
            Assert.That(config.EscalationMaxAliveEnemiesPerCell, Is.EqualTo(8));
            Assert.That(config.AmbientMaxEnemiesPerRequest, Is.EqualTo(4));
            Assert.That(config.AmbientSpawnCooldownSeconds, Is.GreaterThan(0f));
        }

        [Test]
        public void ReplicatedContracts_UseStableGuids()
        {
            Assert.That(new DirectorState().Config().Guid, Is.EqualTo(new Guid("3ee4b71a-e2d1-4446-9301-a36017ab984b")));
            Assert.That(new EnemyArchetype().Config().Guid, Is.EqualTo(new Guid("2fd68c53-cf5c-4d13-94cc-1ad5420a82d7")));
        }

        [Test]
        public void Contracts_RemainCompactEnoughForNetworkAndRuntimeUse()
        {
            Assert.That(Marshal.SizeOf<CombatCell>(), Is.LessThanOrEqualTo(24));
            Assert.That(Marshal.SizeOf<CellAttention>(), Is.LessThanOrEqualTo(32));
            Assert.That(Marshal.SizeOf<ThreatBudget>(), Is.LessThanOrEqualTo(16));
            Assert.That(Marshal.SizeOf<DirectorState>(), Is.LessThanOrEqualTo(16));
            Assert.That(Marshal.SizeOf<EncounterState>(), Is.LessThanOrEqualTo(24));
            Assert.That(Marshal.SizeOf<SpawnSource>(), Is.LessThanOrEqualTo(40));
            Assert.That(Marshal.SizeOf<AmbientSpawnMarker>(), Is.LessThanOrEqualTo(20));
            Assert.That(Marshal.SizeOf<CellAliveEnemyCaps>(), Is.LessThanOrEqualTo(32));
            Assert.That(Marshal.SizeOf<EnemyArchetype>(), Is.LessThanOrEqualTo(8));
            Assert.That(Marshal.SizeOf<SpawnRequest>(), Is.LessThanOrEqualTo(40));
        }
    }
}
