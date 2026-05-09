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
            var sourcePeer = new NetworkPeerId(7);
            var player = scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem();
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
    }
}
