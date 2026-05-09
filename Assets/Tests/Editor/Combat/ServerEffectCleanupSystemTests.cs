using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerEffectCleanupSystemTests
    {
        [Test]
        public void Update_RemovesProcessedEffects_AndKeepsUnprocessedOnes()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntityWithHealth();
            var processed = EffectCommands.CreateDamage(source.GID, target.GID, 5f, DamageType.Physical);
            var pending = EffectCommands.CreateDamage(source.GID, target.GID, 8f, DamageType.Fire);
            var processedGid = processed.GID;
            var pendingGid = pending.GID;
            processed.Set<EffectProcessedTag>();

            var system = new ServerEffectCleanupSystem();
            system.Update();

            Assert.That(processedGid.TryUnpack<ServerWT>(out _), Is.False);
            Assert.That(pendingGid.TryUnpack<ServerWT>(out var pendingEntity), Is.True);
            Assert.That(pendingEntity.Has<EffectProcessedTag>(), Is.False);
        }
    }
}
