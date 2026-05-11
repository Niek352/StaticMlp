using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientPassiveAutoAttackPresentationSystemTests
    {
        [Test]
        public void Update_CreatesTracerSnapshotAndTargetHighlight()
        {
            using var scope = new CombatTestClientWorldScope();
            var now = 30f;
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var target = scope.CreateMonster(new Vector3(2f, 0f, 0f));
            scope.SetGameTime(now, 0f);
            var targeting = new ClientPassiveAutoAttackTargetingSystem();
            var intent = new ClientPassiveAutoAttackIntentSystem();
            var presentation = new ClientPassiveAutoAttackPresentationSystem();

            targeting.Update();
            intent.Update();
            presentation.Update();

            Assert.That(player.Has<PassiveAutoAttackViewState>(), Is.True);
            var tracer = player.Read<PassiveAutoAttackViewState>();
            Assert.That(tracer.HasTracer, Is.True);
            Assert.That(tracer.TracerStart, Is.EqualTo(Vector3.up));
            Assert.That(tracer.TracerEnd, Is.EqualTo(new Vector3(2f, 1f, 0f)));

            Assert.That(target.Has<PassiveAutoAttackTargetViewState>(), Is.True);
            var highlight = target.Read<PassiveAutoAttackTargetViewState>();
            Assert.That(highlight.IsHighlighted, Is.True);
            Assert.That(highlight.HighlightIntensity, Is.EqualTo(1f));
        }

        [Test]
        public void Update_WhenTargetSwitches_ClearsOldHighlightAndActivatesNewOne()
        {
            using var scope = new CombatTestClientWorldScope();
            var now = 40f;
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var first = scope.CreateMonster(new Vector3(2f, 0f, 0f));
            var second = scope.CreateMonster(new Vector3(4f, 0f, 0f));
            scope.SetGameTime(now, scope.PresentationConfig.HighlightFadeOut);
            var targeting = new ClientPassiveAutoAttackTargetingSystem();
            var intent = new ClientPassiveAutoAttackIntentSystem();
            var presentation = new ClientPassiveAutoAttackPresentationSystem();

            targeting.Update();
            intent.Update();
            presentation.Update();

            ref var firstState = ref first.Mut<CharacterNetState>();
            firstState.Position = new Vector3(30f, 0f, 0f);
            now += scope.Config.PoisonArrowCooldown;

            scope.SetGameTime(now, scope.PresentationConfig.HighlightFadeOut);
            targeting.Update();
            intent.Update();
            presentation.Update();

            var oldHighlight = first.Read<PassiveAutoAttackTargetViewState>();
            var newHighlight = second.Read<PassiveAutoAttackTargetViewState>();
            Assert.That(oldHighlight.IsHighlighted, Is.False);
            Assert.That(oldHighlight.HighlightIntensity, Is.EqualTo(0f));
            Assert.That(newHighlight.IsHighlighted, Is.True);
            Assert.That(newHighlight.HighlightIntensity, Is.EqualTo(1f));
        }
    }
}
