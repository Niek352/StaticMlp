using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication.Generated;

namespace StaticMlp.Tests.Combat
{
    public sealed class StatusReplicationGeneratedTests
    {
        [Test]
        public void StatusTargetReplication_RoundTripsEntityGidWithoutRawProxyField()
        {
            using var scope = new CombatTestServerWorldScope();
            var targetEntity = scope.CreateEntity();
            var ownerEntity = scope.CreateEntity();
            var source = new StatusTarget
            {
                Value = targetEntity.GID,
            };

            var delta = StatusTargetReplication.CreateDelta(ownerEntity.GID, source);
            var restored = StatusTargetReplication.Read(delta.Payload);

            Assert.That(restored.Value, Is.EqualTo(source.Value));
            
        }

        [Test]
        public void StatusContextReplication_RoundTripsEntityGidAndChainMetadata()
        {
            using var scope = new CombatTestServerWorldScope();
            var sourceEntity = scope.CreateEntity();
            var ownerEntity = scope.CreateEntity();
            var source = new StatusContext
            {
                Source = sourceEntity.GID,
                RequestId = 17u,
                RootEffectId = 19u,
                ChainDepth = 2,
                MaxDepth = 5,
            };

            var delta = StatusContextReplication.CreateDelta(ownerEntity.GID, source);
            var restored = StatusContextReplication.Read(delta.Payload);

            Assert.That(restored.Source, Is.EqualTo(source.Source));
            Assert.That(restored.RequestId, Is.EqualTo(17u));
            Assert.That(restored.RootEffectId, Is.EqualTo(19u));
            Assert.That(restored.ChainDepth, Is.EqualTo((byte)2));
            Assert.That(restored.MaxDepth, Is.EqualTo((byte)5));
        }
    }
}
