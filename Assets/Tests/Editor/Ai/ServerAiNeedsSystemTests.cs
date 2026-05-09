using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class ServerAiNeedsSystemTests
    {
        [Test]
        public void Update_DerivesHealth01FromReplicatedHealth()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            ref var health = ref ReplicationMut.Mut<Health>(bot);
            health.Current = 25f;
            health.Max = 100f;

            var system = new ServerAiNeedsSystem();
            system.Update();

            Assert.That(AiBlackboardAccess.GetFloat(bot, AiCoreVariableIds.Health01), Is.EqualTo(0.25f));
        }
    }
}
