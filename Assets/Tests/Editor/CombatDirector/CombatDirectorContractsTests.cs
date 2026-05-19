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
            Assert.That((byte)DirectorPhase.Calm, Is.EqualTo(0));
            Assert.That((byte)DirectorPhase.BuildUp, Is.EqualTo(1));
            Assert.That((byte)DirectorPhase.Peak, Is.EqualTo(2));
            Assert.That((byte)DirectorPhase.Relief, Is.EqualTo(3));
            Assert.That((byte)DirectorPhase.Cooldown, Is.EqualTo(4));
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
        public void ReplicatedContracts_UseStableGuids()
        {
            Assert.That(new DirectorState().Config().Guid, Is.EqualTo(new Guid("3ee4b71a-e2d1-4446-9301-a36017ab984b")));
            Assert.That(new EnemyArchetype().Config().Guid, Is.EqualTo(new Guid("2fd68c53-cf5c-4d13-94cc-1ad5420a82d7")));
        }

        [Test]
        public void Contracts_RemainCompactEnoughForNetworkAndRuntimeUse()
        {
            Assert.That(Marshal.SizeOf<CombatCell>(), Is.LessThanOrEqualTo(24));
            Assert.That(Marshal.SizeOf<ThreatBudget>(), Is.LessThanOrEqualTo(16));
            Assert.That(Marshal.SizeOf<DirectorState>(), Is.LessThanOrEqualTo(16));
            Assert.That(Marshal.SizeOf<SpawnSource>(), Is.LessThanOrEqualTo(24));
            Assert.That(Marshal.SizeOf<EnemyArchetype>(), Is.LessThanOrEqualTo(8));
            Assert.That(Marshal.SizeOf<SpawnRequest>(), Is.LessThanOrEqualTo(32));
        }
    }
}
