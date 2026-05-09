using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerUseAbilityCommandPipelineTests
    {
        [Test]
        public void Update_FromUseAbilityCommand_BasicMeleeAuto_ReducesTargetHealth()
        {
            using var scope = new CombatTestServerWorldScope();
            const float now = 4f;
            var sourcePeer = new NetworkPeerId(21);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f));
            var receive = new ServerReceiveCombatCommandsSystem();

            receive.Init();
            SW.SendEvent(new NetworkEventFromClient<UseAbilityCommand>(
                sourcePeer,
                new UseAbilityCommand(CombatAbilityId.BasicMeleeAuto, target.GID, 11u)));

            receive.Update();
            new ServerValidateCombatCommandsSystem(() => now).Update();
            new ServerAbilityCastSystem().Update();
            new ServerHitToEffectSystem().Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerDamageApplySystem().Update();
            receive.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_PoisonArrow_AddsPoisonStatus_AndTickCreatesDamage()
        {
            using var scope = new CombatTestServerWorldScope();
            const float now = 4f;
            var sourcePeer = new NetworkPeerId(22);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f));
            var receive = new ServerReceiveCombatCommandsSystem();

            receive.Init();
            SW.SendEvent(new NetworkEventFromClient<UseAbilityCommand>(
                sourcePeer,
                new UseAbilityCommand(CombatAbilityId.PoisonArrow, target.GID, 12u)));

            receive.Update();
            new ServerValidateCombatCommandsSystem(() => now).Update();
            new ServerAbilityCastSystem().Update();
            new ServerHitToEffectSystem().Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerAddStatusApplySystem().Update();
            new ServerDamageApplySystem().Update();

            Assert.That(target.Has<PoisonStatus>(), Is.True);
            Assert.That(target.Read<Health>().Current, Is.EqualTo(92f));

            new ServerStatusTickSystem(() => 1f).Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerDamageApplySystem().Update();
            receive.Destroy();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(89f));
        }

        [Test]
        public void Update_FireFlaskAgainstOiledTarget_TriggersBurningAndBurningPool()
        {
            using var scope = new CombatTestServerWorldScope();
            const float now = 4f;
            var sourcePeer = new NetworkPeerId(23);
            scope.CreatePlayer(sourcePeer, Vector3.zero);
            var target = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 0f));
            target.Set(new OiledStatus
            {
                RemainingTime = 4f,
                Power = 1f,
                Stacks = 1,
                Source = target.GID,
                RequestId = 0,
                RootEffectId = 1,
                ChainDepth = 0,
                MaxDepth = 4,
            });

            var receive = new ServerReceiveCombatCommandsSystem();
            receive.Init();
            SW.SendEvent(new NetworkEventFromClient<UseAbilityCommand>(
                sourcePeer,
                new UseAbilityCommand(CombatAbilityId.FireFlask, target.GID, 13u)));

            receive.Update();
            new ServerValidateCombatCommandsSystem(() => now).Update();
            new ServerAbilityCastSystem().Update();
            new ServerHitToEffectSystem().Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerSynergyTriggerSystem().Update();
            new ServerAddStatusApplySystem().Update();
            new ServerDamageApplySystem().Update();

            Assert.That(target.Has<BurningStatus>(), Is.True);
            Assert.That(target.Has<OiledStatus>(), Is.False);

            new ServerAreaEffectTickSystem(() => 1f).Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerDamageApplySystem().Update();
            receive.Destroy();

            Assert.That(target.Read<Health>().Current, Is.LessThan(88f));
        }
    }
}
