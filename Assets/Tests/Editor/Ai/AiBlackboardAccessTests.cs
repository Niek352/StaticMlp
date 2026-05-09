using NUnit.Framework;
using StaticMlp.Features.AiBots;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiBlackboardAccessTests
    {
        [Test]
        public void SetOverwriteAndRemoveEntries_WorkForAllSupportedValueKinds()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            var target = scope.CreateBot(new Vector3(2f, 0f, 0f));

            AiBlackboardAccess.SetFloat(bot, 9001, 0.25f);
            Assert.That(AiBlackboardAccess.TryGetFloat(bot, 9001, out var floatValue), Is.True);
            Assert.That(floatValue, Is.EqualTo(0.25f).Within(0.0001f));

            AiBlackboardAccess.SetFloat(bot, 9001, 0.75f);
            Assert.That(AiBlackboardAccess.GetFloat(bot, 9001), Is.EqualTo(0.75f).Within(0.0001f));

            AiBlackboardAccess.SetEntity(bot, 9002, target.GID);
            Assert.That(AiBlackboardAccess.TryGetEntity(bot, 9002, out var entityValue), Is.True);
            Assert.That(entityValue, Is.EqualTo(target.GID));

            var vector = new Vector3(4f, 5f, 6f);
            AiBlackboardAccess.SetVector(bot, 9003, vector);
            Assert.That(AiBlackboardAccess.TryGetVector(bot, 9003, out var vectorValue), Is.True);
            Assert.That(vectorValue, Is.EqualTo(vector));

            AiBlackboardAccess.Remove(bot, 9001);
            Assert.That(AiBlackboardAccess.Has(bot, 9001), Is.False);
            Assert.That(AiBlackboardAccess.TryGetFloat(bot, 9001, out _), Is.False);
        }
    }
}
