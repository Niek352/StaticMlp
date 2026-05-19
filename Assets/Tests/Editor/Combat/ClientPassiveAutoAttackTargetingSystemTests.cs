using NUnit.Framework;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientPassiveAutoAttackTargetingSystemTests
    {
        [Test]
        public void Update_WithNoMonstersInRadius_KeepsTargetEmpty()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            scope.CreateMonster(new Vector3(20f, 0f, 0f));
            scope.SetGameTime(1f);
            var system = new ClientPassiveAutoAttackTargetingSystem();
            var intent = new ClientPassiveAutoAttackIntentSystem();

            system.Update();
            intent.Update();

            Assert.That(player.Has<PassiveAutoAttackState>(), Is.True);
            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget.Raw, Is.EqualTo(0ul));
            Assert.That(player.Has<PassiveAutoAttackIntent>(), Is.False);
        }

        [Test]
        public void Update_WithMultipleMonsters_SelectsNearestTarget()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            scope.CreateMonster(new Vector3(6f, 0f, 0f));
            var nearest = scope.CreateMonster(new Vector3(3f, 0f, 0f));
            scope.SetGameTime(2f);
            var system = new ClientPassiveAutoAttackTargetingSystem();

            system.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget, Is.EqualTo(nearest.GID));
        }

        [Test]
        public void Update_WhenDistancesTie_SelectsLowerEntityGid()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var first = scope.CreateMonster(new Vector3(-4f, 0f, 0f));
            scope.CreateMonster(new Vector3(4f, 0f, 0f));
            scope.SetGameTime(3f);
            var system = new ClientPassiveAutoAttackTargetingSystem();

            system.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget, Is.EqualTo(first.GID));
        }

        [Test]
        public void Update_IgnoresMonstersOutsidePreparedBuildRange()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            scope.CreateMonster(new Vector3(10.01f, 0f, 0f));
            scope.SetGameTime(4f);
            var system = new ClientPassiveAutoAttackTargetingSystem();

            system.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget.Raw, Is.EqualTo(0ul));
        }

        [Test]
        public void Update_WhenPreparedBuildChanges_UsesSnapshotRangeInsteadOfCombatDefault()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            player.Set(new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.FireFlaskModuleId
            });
            player.Set(Stage1LoadoutRules.CreatePreparedSnapshot(player.Read<OwnerLoadoutSelection>()));
            scope.CreateMonster(new Vector3(9.5f, 0f, 0f));
            scope.SetGameTime(4.5f);
            var system = new ClientPassiveAutoAttackTargetingSystem();

            system.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget.Raw, Is.EqualTo(0ul));
        }

        [Test]
        public void Update_IgnoresDeadMonstersWhenHealthExists()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            scope.CreateMonster(new Vector3(2f, 0f, 0f), health: 0f);
            var alive = scope.CreateMonster(new Vector3(4f, 0f, 0f), health: 10f);
            scope.SetGameTime(5f);
            var system = new ClientPassiveAutoAttackTargetingSystem();

            system.Update();

            Assert.That(player.Read<PassiveAutoAttackState>().CurrentTarget, Is.EqualTo(alive.GID));
        }
    }
}
