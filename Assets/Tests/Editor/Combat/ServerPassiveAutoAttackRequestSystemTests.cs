using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerPassiveAutoAttackRequestSystemTests
    {
        [Test]
        public void Update_FromPlayerAutoAttackEvent_CreatesDamageAndReducesMonsterHealth()
        {
            using var scope = new CombatTestServerWorldScope();
            const float now = 10f;
            var sourcePeer = new NetworkPeerId(7);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem(() => now);
            var applySystem = new ServerDamageApplySystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 42u)));

            requestSystem.Update();
            applySystem.Update();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RejectsDuplicateShotSequence()
        {
            using var scope = new CombatTestServerWorldScope();
            const float now = 15f;
            var sourcePeer = new NetworkPeerId(8);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem(() => now);
            var applySystem = new ServerDamageApplySystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 7u)));
            requestSystem.Update();
            applySystem.Update();

            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 7u)));
            requestSystem.Update();
            applySystem.Update();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RejectsAttackBeforeServerFireInterval()
        {
            using var scope = new CombatTestServerWorldScope();
            var now = 20f;
            var sourcePeer = new NetworkPeerId(9);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem(() => now);
            var applySystem = new ServerDamageApplySystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 1u)));
            requestSystem.Update();
            applySystem.Update();

            now += 0.1f;
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 2u)));
            requestSystem.Update();
            applySystem.Update();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }
    }
}
