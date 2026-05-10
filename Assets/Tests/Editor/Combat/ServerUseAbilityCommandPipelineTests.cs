using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
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
            new ServerApplyPoisonStatusSystem().Update();
            new ServerApplyBurningStatusSystem().Update();
            new ServerApplyOiledStatusSystem().Update();
            new ServerDamageApplySystem().Update();

            Assert.That(StatusEntityLookup.TryFind<PoisonStatus>(target.GID, out _), Is.True);
            Assert.That(target.Read<Health>().Current, Is.EqualTo(92f));

            new ServerPoisonStatusTickSystem(() => 1f).Update();
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
            StatusEntitySpawns.SpawnOiled(target, target.GID, new AddStatusSpec
            {
                Duration = 4f,
                TickInterval = 0f,
                Power = 1f,
                Stacks = 1,
            }, 0, new EffectChainData
            {
                RootEffectId = 1,
                Depth = 0,
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
            new ServerApplyPoisonStatusSystem().Update();
            new ServerApplyBurningStatusSystem().Update();
            new ServerApplyOiledStatusSystem().Update();
            new ServerDamageApplySystem().Update();

            Assert.That(StatusEntityLookup.TryFind<BurningStatus>(target.GID, out _), Is.True);
            Assert.That(StatusEntityLookup.TryFind<OiledStatus>(target.GID, out _), Is.False);

            new ServerAreaEffectTickSystem(() => 1f).Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerDamageApplySystem().Update();
            receive.Destroy();

            Assert.That(target.Read<Health>().Current, Is.LessThan(88f));
        }

        [Test]
        public void Update_PoisonEffectsMergeIntoSingleStatusEntity()
        {
            using var scope = new CombatTestServerWorldScope();
            var source = scope.CreateEntity();
            var target = scope.CreateMonsterWithHealth(Vector3.zero);

            StatusEffectCommands.CreatePoisonStatus(source.GID, target.GID, 6f, 1f, 3f, 1, requestId: 11, rootEffectId: 11, depth: 1, maxDepth: 4);
            StatusEffectCommands.CreatePoisonStatus(source.GID, target.GID, 4f, 0.5f, 5f, 2, requestId: 12, rootEffectId: 12, depth: 2, maxDepth: 5);

            new ServerEffectPreprocessSystem().Update();
            new ServerApplyPoisonStatusSystem().Update();

            Assert.That(StatusEntityLookup.TryFind<PoisonStatus>(target.GID, out var statusEntity), Is.True);
            var poisonStatusCount = 0;
            foreach (var _ in SW.Query<All<PoisonStatus, StatusTarget>>().Entities())
                poisonStatusCount++;

            Assert.That(poisonStatusCount, Is.EqualTo(1));
            Assert.That(statusEntity.Read<LifeTime>().RemainingTime, Is.EqualTo(6f));
            Assert.That(statusEntity.Read<StatusStrength>().Power, Is.EqualTo(5f));
            Assert.That(statusEntity.Read<StatusStrength>().Stacks, Is.EqualTo(3));
            Assert.That(statusEntity.Read<StatusTickState>().Interval, Is.EqualTo(0.5f));
            Assert.That(statusEntity.Read<StatusContext>().RequestId, Is.EqualTo(12u));
            Assert.That(statusEntity.Read<StatusContext>().RootEffectId, Is.EqualTo(12u));
            Assert.That(statusEntity.Read<StatusContext>().ChainDepth, Is.EqualTo(2));
            Assert.That(statusEntity.Read<StatusContext>().MaxDepth, Is.EqualTo(5));
        }
    }
}
