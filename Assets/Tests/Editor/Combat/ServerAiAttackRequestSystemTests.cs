using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerAiAttackRequestSystemTests
    {
        private static void RunCombatPipeline()
        {
            new ServerValidateCombatCommandsSystem().Update();
            new ServerAbilityCastSystem().Update();
            new ServerHitToEffectSystem().Update();
            new ServerEffectPreprocessSystem().Update();
            new ServerDamageApplySystem().Update();
            new ServerEffectCleanupSystem().Update();
        }

        [Test]
        public void Update_FromAiAttackRequest_CreatesSharedCombatRequest_AndReducesTargetHealth()
        {
            using var scope = new CombatTestServerWorldScope();
            scope.SetSimulationTime(150);
            var attacker = scope.CreateEntity();
            attacker.Set<ServerOwned>();
            attacker.Set<AiAgentTag>();
            attacker.Set(new CharacterNetState
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            var target = scope.CreateEntityWithHealth(100f, 100f);
            target.Set(new CharacterNetState
            {
                Position = new Vector3(2f, 0f, 0f),
                Rotation = Quaternion.identity
            });

            attacker.Set(new AiAttackRequest
            {
                Target = target.GID
            });
            attacker.Set(new ServerCombatAttackState());

            var requestSystem = new ServerAiAttackRequestSystem();
            requestSystem.Update();
            RunCombatPipeline();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RespectsServerFireIntervalForAiAttack()
        {
            using var scope = new CombatTestServerWorldScope();
            scope.SetSimulationTime(240);
            var attacker = scope.CreateEntity();
            attacker.Set<ServerOwned>();
            attacker.Set<AiAgentTag>();
            attacker.Set(new CharacterNetState
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });

            var target = scope.CreateEntityWithHealth(100f, 100f);
            target.Set(new CharacterNetState
            {
                Position = new Vector3(2f, 0f, 0f),
                Rotation = Quaternion.identity
            });

            attacker.Set(new AiAttackRequest
            {
                Target = target.GID
            });
            attacker.Set(new ServerCombatAttackState());

            var requestSystem = new ServerAiAttackRequestSystem();
            requestSystem.Update();
            RunCombatPipeline();

            scope.AdvanceSimulationSeconds(0.1f);
            requestSystem.Update();
            RunCombatPipeline();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }
    }
}
