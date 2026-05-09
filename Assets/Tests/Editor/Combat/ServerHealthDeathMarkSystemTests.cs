using NUnit.Framework;
using StaticMlp.Features.Combat;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerHealthDeathMarkSystemTests
    {
        [Test]
        public void Update_WhenHealthIsZero_AddsDeathTag()
        {
            using var scope = new CombatTestServerWorldScope();
            var target = scope.CreateEntityWithHealth(current: 0f, max: 100f);

            var system = new ServerHealthDeathMarkSystem();
            system.Update();

            Assert.That(target.Has<IsDiedTag>(), Is.True);
        }

        [Test]
        public void Update_WhenHealthIsAboveZero_DoesNotAddDeathTag()
        {
            using var scope = new CombatTestServerWorldScope();
            var target = scope.CreateEntityWithHealth(current: 25f, max: 100f);

            var system = new ServerHealthDeathMarkSystem();
            system.Update();

            Assert.That(target.Has<IsDiedTag>(), Is.False);
        }
    }
}
