using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerPassiveAutoAttackRequestSystemTests
    {
        private static void RunCombatPipeline()
        {
            new ServerValidateCombatCommandsSystem().Update();
            new ServerAbilityCastSystem().Update();
            new ServerHitToEffectSystem().Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerSynergyTriggerSystem().Update();
            new ServerApplyPoisonStatusSystem().Update();
            new ServerApplyBurningStatusSystem().Update();
            new ServerApplyOiledStatusSystem().Update();
            new ServerDamageApplySystem().Update();
            new ServerEffectCleanupSystem().Update();
        }

        [Test]
        public void Update_FromPlayerAutoAttackEvent_CreatesRequest_AndSharedPipelineReducesMonsterHealth()
        {
            using var scope = new CombatTestServerWorldScope();
            scope.SetSimulationTime(300);
            var sourcePeer = new NetworkPeerId(7);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 42u)));

            requestSystem.Update();
            RunCombatPipeline();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RejectsDuplicateShotSequence()
        {
            using var scope = new CombatTestServerWorldScope();
            scope.SetSimulationTime(450);
            var sourcePeer = new NetworkPeerId(8);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 7u)));
            requestSystem.Update();
            RunCombatPipeline();

            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 7u)));
            requestSystem.Update();
            RunCombatPipeline();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RejectsAttackBeforeServerFireInterval()
        {
            using var scope = new CombatTestServerWorldScope();
            scope.SetSimulationTime(600);
            var sourcePeer = new NetworkPeerId(9);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f), current: 100f, max: 100f);
            var requestSystem = new ServerPassiveAutoAttackRequestSystem();

            requestSystem.Init();
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 1u)));
            requestSystem.Update();
            RunCombatPipeline();

            scope.AdvanceSimulationSeconds(0.1f);
            SW.SendEvent(new NetworkEventFromClient<PassiveAutoAttackRequestEvent>(
                sourcePeer,
                new PassiveAutoAttackRequestEvent(target.GID, 2u)));
            requestSystem.Update();
            RunCombatPipeline();
            requestSystem.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }
    }
}
