using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerAiAttackRequestSystemTests
    {
        [Test]
        public void Update_FromAiAttackRequest_CreatesDamageAndReducesTargetHealth()
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

            var requestSystem = new ServerAiAttackRequestSystem(() => now);
            var applySystem = new ServerDamageApplySystem();

            requestSystem.Update();
            applySystem.Update();

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

            var requestSystem = new ServerAiAttackRequestSystem(() => now);
            var applySystem = new ServerDamageApplySystem();

            requestSystem.Update();
            applySystem.Update();

            now += 0.1f;
            requestSystem.Update();
            applySystem.Update();

            Assert.That(target.Read<Health>().Current, Is.EqualTo(90f));
        }
    }
}
