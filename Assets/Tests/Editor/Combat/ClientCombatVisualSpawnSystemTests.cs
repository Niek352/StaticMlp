using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Features.EcsViews;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientCombatVisualSpawnSystemTests
    {
        [Test]
        public void Update_ForFireFlaskIntent_SpawnsProjectileVisualEntity()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var target = scope.CreateMonster(new Vector3(3f, 0f, 0f));

            player.Set(new PassiveAutoAttackIntent
            {
                AbilityId = CombatAbilityId.FireFlask,
                Target = target.GID,
                ShotSequence = 4u,
                LocalFireTime = 1f,
            });
            player.Set(new LocalCombatPredictionState());

            var system = new ClientCombatVisualSpawnSystem();
            system.Update();

            var found = false;
            foreach (var entity in CW.Query<All<ViewPath, CombatProjectileVisualState>>().Entities())
            {
                found = true;
                Assert.That(entity.Read<ViewPath>().Value, Is.EqualTo("Views/Combat/CombatProjectileView"));
                Assert.That(entity.Read<CombatProjectileVisualState>().AbilityId, Is.EqualTo(CombatAbilityId.FireFlask));
            }

            Assert.That(found, Is.True);
        }
    }
}
