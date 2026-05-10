using System.Linq;
using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerDamageApplySystemTests
    {
        [Test]
        public void Update_ReducesHealth_AndMarksTargetDirtyForReplication()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntityWithHealth(current: 100f, max: 100f);
            var effect = EffectCommands.CreateDamage(source.GID, target.GID, 25f, DamageType.Physical);

            var system = new ServerDamageApplySystem();
            system.Update();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(75f));
            Assert.That(target.Has<NetworkDirty>(), Is.True);
            Assert.That(effect.Has<EffectProcessedTag>(), Is.True);
            Assert.That(scope.DebugLog.Entries.Any(x => x.Contains("apply damage")), Is.True);
        }

        [Test]
        public void Update_ClampsHealthAtZero()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntityWithHealth(current: 10f, max: 100f);

            EffectCommands.CreateDamage(source.GID, target.GID, 50f, DamageType.Fire);

            var system = new ServerDamageApplySystem();
            system.Update();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(0f));
        }

        [Test]
        public void Update_WhenTargetWasDestroyed_ConsumesEffectAndLogsDiscard()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntityWithHealth();
            var targetGid = target.GID;
            target.Destroy();

            var effect = EffectCommands.CreateDamage(source.GID, targetGid, 12f, DamageType.Poison);

            var system = new ServerDamageApplySystem();
            system.Update();

            Assert.That(effect.Has<EffectProcessedTag>(), Is.True);
            Assert.That(scope.DebugLog.Entries.Any(x => x.Contains("discard damage")), Is.True);
        }

        [Test]
        public void Update_WhenTargetHasNoHealth_ThrowsClearException()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntity();

            EffectCommands.CreateDamage(source.GID, target.GID, 5f, DamageType.Physical);

            var system = new ServerDamageApplySystem();
            var exception = Assert.Throws<System.InvalidOperationException>(() => system.Update());

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Does.Contain("missing required Health component"));
        }
    }
}
