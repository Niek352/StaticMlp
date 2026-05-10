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
            var source = new StatusTarget
            {
                Value = new EntityGID(0x0102030405060708ul),
            };

            var delta = StatusTargetReplication.CreateDelta(new EntityGID(77ul), source);
            var restored = StatusTargetReplication.Read(delta.Payload);

            Assert.That(restored.Value, Is.EqualTo(source.Value));
            
        }

        [Test]
        public void StatusContextReplication_RoundTripsEntityGidAndChainMetadata()
        {
            var source = new StatusContext
            {
                Source = new EntityGID(0x0908070605040302ul),
                RequestId = 17u,
                RootEffectId = 19u,
                ChainDepth = 2,
                MaxDepth = 5,
            };

            var delta = StatusContextReplication.CreateDelta(new EntityGID(88ul), source);
            var restored = StatusContextReplication.Read(delta.Payload);

            Assert.That(restored.Source, Is.EqualTo(source.Source));
            Assert.That(restored.RequestId, Is.EqualTo(17u));
            Assert.That(restored.RootEffectId, Is.EqualTo(19u));
            Assert.That(restored.ChainDepth, Is.EqualTo((byte)2));
            Assert.That(restored.MaxDepth, Is.EqualTo((byte)5));
        }
    }
}
