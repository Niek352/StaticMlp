using System;
using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientCombatViewStateSystemTests
    {
        [Test]
        public void Update_ForCombatActorWithoutHealth_ThrowsHelpfulException()
        {
            using var scope = new CombatTestClientWorldScope();
            scope.CreateMonster(Vector3.zero, health: null);

            var system = new ClientCombatViewStateSystem();

            var exception = Assert.Throws<InvalidOperationException>(() => system.Update());
            Assert.That(exception!.Message, Does.Contain("missing replicated Health"));
            Assert.That(exception.Message, Does.Contain(nameof(MonsterTag)));
        }

        [Test]
        public void Update_ForCombatActorWithHealth_WritesCombatViewState()
        {
            using var scope = new CombatTestClientWorldScope();
            var monster = scope.CreateMonster(Vector3.zero, health: 25f);
            ref var health = ref monster.Mut<Health>();
            health.Max = 100f;

            var system = new ClientCombatViewStateSystem();
            system.Update();

            ref readonly var state = ref monster.Read<CombatViewState>();
            Assert.That(state.HealthNormalized, Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(state.IsDead, Is.False);
        }
    }
}
