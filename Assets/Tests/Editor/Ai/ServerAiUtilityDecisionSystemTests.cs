using NUnit.Framework;
using StaticMlp.Features.AiBots;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class ServerAiUtilityDecisionSystemTests
    {
        [Test]
        public void Update_RespectsNextDecisionTick()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            scope.SimulationTime.ServerTick = 5;

            ref var brain = ref bot.Mut<AiBrain>();
            brain.NextDecisionTick = 10;

            new ServerAiUtilityDecisionSystem().Update();

            Assert.That(bot.Read<AiBrain>().NextDecisionTick, Is.EqualTo(10u));

            scope.SimulationTime.ServerTick = 10;
            new ServerAiUtilityDecisionSystem().Update();

            Assert.That(bot.Read<AiBrain>().NextDecisionTick, Is.EqualTo(18u));
        }
    }
}
