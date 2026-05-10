using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerAiAttackRequestSystemTests
    {
        private static void RunCombatPipeline(float now)
        {
            new ServerValidateCombatCommandsSystem(() => now).Update();
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
            const float now = 5f;
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
            RunCombatPipeline(now);

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }

        [Test]
        public void Update_RespectsServerFireIntervalForAiAttack()
        {
            using var scope = new CombatTestServerWorldScope();
            var now = 8f;
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
            RunCombatPipeline(now);

            now += 0.1f;
            requestSystem.Update();
            RunCombatPipeline(now);

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }
    }
}
