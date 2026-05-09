using NUnit.Framework;
using StaticMlp.Features.Combat;

namespace StaticMlp.Tests.Combat
{
    public sealed class EffectCommandsTests
    {
        [Test]
        public void CreateDamage_CreatesEffectEntityWithExpectedContract()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateEntity();

            var effect = EffectCommands.CreateDamage(source.GID, target.GID, 25f, DamageType.Fire, 7);

            Assert.That(effect.EntityType, Is.EqualTo(new EffectEntityType().Id()));
            Assert.That(effect.Has<EffectTag>(), Is.True);
            Assert.That(effect.Has<DamageEffectTag>(), Is.True);
            Assert.That(effect.Read<EffectKind>().Value, Is.EqualTo(EffectType.Damage));
            Assert.That(effect.Read<EffectSource>().Value, Is.EqualTo(source.GID));
            Assert.That(effect.Read<EffectTarget>().Value, Is.EqualTo(target.GID));
            Assert.That(effect.Read<EffectValue>().Value, Is.EqualTo(25f));
            Assert.That(effect.Read<EffectCreatedTick>().Tick, Is.EqualTo(0u));
            Assert.That(effect.Read<EffectRequestId>().Value, Is.EqualTo(7u));
            Assert.That(effect.Read<DamageData>().Type, Is.EqualTo(DamageType.Fire));
        }
    }
}
