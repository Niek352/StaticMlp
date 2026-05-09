using NUnit.Framework;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientDamageFeedbackPresentationSystemTests
    {
        [Test]
        public void Update_WhenHealthDrops_CreatesActiveDamageFeedback()
        {
            using var scope = new CombatTestClientWorldScope();
            var target = scope.CreateMonster(new Vector3(2f, 0f, 0f), health: 100f);
            var system = new ClientDamageFeedbackPresentationSystem(() => 0f);

            system.Update();

            ref var health = ref target.Mut<Health>();
            health.Current = 75f;
            system.Update();

            Assert.That(target.Has<DamageFeedbackViewState>(), Is.True);
            var feedback = target.Read<DamageFeedbackViewState>();
            Assert.That(feedback.IsActive, Is.True);
            Assert.That(feedback.DamageAmount, Is.EqualTo(25f));
            Assert.That(feedback.Intensity, Is.EqualTo(1f));
        }

        [Test]
        public void Update_WhenHealthDoesNotDrop_DoesNotCreateFeedback()
        {
            using var scope = new CombatTestClientWorldScope();
            scope.CreateMonster(new Vector3(2f, 0f, 0f), health: 100f);
            var system = new ClientDamageFeedbackPresentationSystem(() => 0f);

            system.Update();
            system.Update();

            foreach (var entity in CW.Query<All<DamageFeedbackViewState>>().Entities())
                Assert.Fail($"Unexpected damage feedback on entity {entity.GID.Raw}.");
        }

        [Test]
        public void Update_DecaysFeedbackUntilInactive()
        {
            using var scope = new CombatTestClientWorldScope();
            var target = scope.CreateMonster(new Vector3(2f, 0f, 0f), health: 100f);
            var system = new ClientDamageFeedbackPresentationSystem(() => scope.Config.DamageFlashLifetime);

            system.Update();

            ref var health = ref target.Mut<Health>();
            health.Current = 90f;
            system.Update();
            system.Update();

            var feedback = target.Read<DamageFeedbackViewState>();
            Assert.That(feedback.IsActive, Is.False);
            Assert.That(feedback.Intensity, Is.EqualTo(0f));
        }
    }
}
