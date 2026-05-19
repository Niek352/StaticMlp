using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class ServerAiPerceptionSystemTests
    {
        [Test]
        public void Update_AssignedDistantEnemy_RemainsTracked()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            var player = CreatePlayer(new Vector3(80f, 0f, 0f));
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, player.GID);

            new ServerAiPerceptionSystem().Update();

            Assert.That(AiBlackboardAccess.TryGetEntity(bot, AiCoreVariableIds.Enemy, out var target), Is.True);
            Assert.That(target, Is.EqualTo(player.GID));
            Assert.That(
                AiBlackboardAccess.GetFloat(bot, AiCoreVariableIds.EnemyDistance),
                Is.EqualTo(80f).Within(0.001f));
        }

        [Test]
        public void Update_UnassignedDistantEnemy_IsNotAcquired()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            CreatePlayer(new Vector3(80f, 0f, 0f));

            new ServerAiPerceptionSystem().Update();

            Assert.That(AiBlackboardAccess.TryGetEntity(bot, AiCoreVariableIds.Enemy, out _), Is.False);
            Assert.That(
                AiBlackboardAccess.GetFloat(bot, AiCoreVariableIds.EnemyDistance),
                Is.EqualTo(999f).Within(0.001f));
        }

        private static SW.Entity CreatePlayer(Vector3 position)
        {
            var player = SW.NewEntity<Default>();
            player.Set<PlayerTag>();
            player.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            return player;
        }
    }
}
