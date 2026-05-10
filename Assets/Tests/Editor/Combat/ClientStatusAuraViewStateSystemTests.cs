using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientStatusAuraViewStateSystemTests
    {
        [Test]
        public void Update_AggregatesReplicatedStatusEntitiesByTarget()
        {
            using var scope = new CombatTestClientWorldScope();
            var monster = scope.CreateMonster(Vector3.zero, health: 100f);

            var poison = CW.NewEntity<Default>();
            poison.Set<PoisonStatus>();
            poison.Set(new StatusTarget { Value = monster.GID });

            var burning = CW.NewEntity<Default>();
            burning.Set<BurningStatus>();
            burning.Set(new StatusTarget { Value = monster.GID });

            var system = new ClientStatusAuraViewStateSystem();
            system.Update();

            var aura = monster.Read<StatusAuraViewState>();
            Assert.That(aura.Flags, Is.EqualTo(StatusVisualFlags.Poison | StatusVisualFlags.Burning));
            Assert.That(aura.IsDead, Is.False);
        }
    }
}
