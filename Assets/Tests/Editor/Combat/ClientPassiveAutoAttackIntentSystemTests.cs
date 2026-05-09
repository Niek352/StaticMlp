using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientPassiveAutoAttackIntentSystemTests
    {
        [Test]
        public void Update_WithStableTarget_EmitsOncePerFireInterval()
        {
            using var scope = new CombatTestClientWorldScope();
            var now = 10f;
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var target = scope.CreateMonster(new Vector3(2f, 0f, 0f));
            var targeting = new ClientPassiveAutoAttackTargetingSystem(() => now);
            var intent = new ClientPassiveAutoAttackIntentSystem(() => now);

            targeting.Update();
            intent.Update();

            Assert.That(player.Has<PassiveAutoAttackIntent>(), Is.True);
            Assert.That(player.Read<PassiveAutoAttackIntent>().Target, Is.EqualTo(target.GID));
            Assert.That(player.Read<PassiveAutoAttackIntent>().ShotSequence, Is.EqualTo(1u));

            intent.Update();
            Assert.That(player.Read<PassiveAutoAttackIntent>().ShotSequence, Is.EqualTo(1u));

            now += scope.Config.FireInterval;
            intent.Update();
            Assert.That(player.Read<PassiveAutoAttackIntent>().ShotSequence, Is.EqualTo(2u));
            Assert.That(player.Read<PassiveAutoAttackState>().LastShotSequence, Is.EqualTo(2u));
        }

        [Test]
        public void Update_WhenTargetIsLost_RemovesPendingIntent()
        {
            using var scope = new CombatTestClientWorldScope();
            var now = 20f;
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var target = scope.CreateMonster(new Vector3(2f, 0f, 0f));
            var targeting = new ClientPassiveAutoAttackTargetingSystem(() => now);
            var intent = new ClientPassiveAutoAttackIntentSystem(() => now);

            targeting.Update();
            intent.Update();
            Assert.That(player.Has<PassiveAutoAttackIntent>(), Is.True);

            ref var targetState = ref target.Mut<CharacterNetState>();
            targetState.Position = new Vector3(30f, 0f, 0f);

            now += 0.1f;
            targeting.Update();
            intent.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget.Raw, Is.EqualTo(0ul));
            Assert.That(player.Has<PassiveAutoAttackIntent>(), Is.False);
        }
    }
}
