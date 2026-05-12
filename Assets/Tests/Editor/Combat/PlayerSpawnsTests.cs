using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Features.Player;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class PlayerSpawnsTests
    {
        [Test]
        public void SpawnPlayer_CreatesPlayerWithFullHealth()
        {
            using var scope = new CombatTestServerWorldScope();

            var gid = PlayerSpawns.Spawn(new PlayerSpawnSpec(new NetworkPeerId(1), new Vector3(2f, 0f, 0f), Quaternion.identity));

            Assert.That(gid.TryUnpack<ServerWT>(out var entity), Is.True);
            Assert.That(entity.Has<PlayerTag>(), Is.True);
            Assert.That(entity.Has<Health>(), Is.True);
            Assert.That(entity.Read<Health>().Current, Is.EqualTo(100f));
            Assert.That(entity.Read<Health>().Max, Is.EqualTo(100f));
        }
    }
}
