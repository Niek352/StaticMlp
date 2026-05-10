using NUnit.Framework;
using StaticMlp.Features.Statuses;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Tests.Combat
{
    public sealed class ServerLifecycleSystemsTests
    {
        [SetUp]
        public void SetUp()
        {
            ServerPeerRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            ServerPeerRegistry.Clear();
        }

        [Test]
        public void Update_LifeTimeExpired_AddsIsDestroyed()
        {
            using var scope = new CombatTestServerWorldScope();
            var entity = scope.CreateEntity();
            entity.Set(new LifeTime { RemainingTime = 0f });

            new ServerLifeTimeExpireMarkSystem().Update();

            Assert.That(entity.Has<IsDestroyed>(), Is.True);
        }

        [Test]
        public void Update_DestroyedNonNetworkedEntity_RemovesEntity()
        {
            using var scope = new CombatTestServerWorldScope();
            var entity = scope.CreateEntity();
            var gid = entity.GID;
            entity.Set<IsDestroyed>();

            new ServerDestroyedEntityCleanupSystem().Update();

            Assert.That(gid.TryUnpack<ServerWT>(out _), Is.False);
        }

        [Test]
        public void Update_DestroyedNetworkedEntity_SendsDespawnAndRemovesEntity()
        {
            using var scope = new CombatTestServerWorldScope();
            var entity = scope.CreateEntity();
            var gid = entity.GID;
            var peer = new NetworkPeerId(41);
            var outbox = new NetOutbox();
            SW.SetResource(outbox);
            ServerPeerRegistry.Add(peer);

            entity.Set<NetworkedTag>();
            entity.Set(new NetworkIdentity
            {
                Owner = peer,
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = StatusNetworkArchetypes.POISON,
            });
            entity.Set<IsDestroyed>();

            new ServerDestroyedEntityCleanupSystem().Update();

            Assert.That(gid.TryUnpack<ServerWT>(out _), Is.False);
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            Assert.That(outbox.Packets[0].Peer, Is.EqualTo(peer));
            CollectionAssert.AreEqual(
                PacketCodec.EncodeDespawn(new DespawnMessage(gid)),
                outbox.Packets[0].Payload);
        }
    }
}
