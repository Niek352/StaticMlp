using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class ServerBotAiDeathSystemTests
    {
        [Test]
        public void Update_RemovesOnlyDeadBots()
        {
            using var scope = new AiTestServerWorldScope();
            var deadBot = scope.CreateBot(Vector3.zero);
            var deadBotGid = deadBot.GID;
            deadBot.Set<IsDiedTag>();

            var aliveBot = scope.CreateBot(Vector3.right);
            var aliveBotGid = aliveBot.GID;

            var deadNonBot = SW.NewEntity<Default>();
            var deadNonBotGid = deadNonBot.GID;
            deadNonBot.Set<IsDiedTag>();

            var system = new ServerBotAiDeathSystem();
            system.Update();

            Assert.That(deadBotGid.TryUnpack<ServerWT>(out _), Is.False);
            Assert.That(aliveBotGid.TryUnpack<ServerWT>(out var aliveBotEntity), Is.True);
            Assert.That(aliveBotEntity.Has<AiAgentTag>(), Is.True);
            Assert.That(deadNonBotGid.TryUnpack<ServerWT>(out var deadNonBotEntity), Is.True);
            Assert.That(deadNonBotEntity.Has<IsDiedTag>(), Is.True);
        }
    }
}
