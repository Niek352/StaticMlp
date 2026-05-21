using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using StaticMlp.Features.CombatDirector;

namespace StaticMlp.Tests.CombatDirector
{
    public sealed class CombatDirectorContractsTests
    {

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
    }
}
