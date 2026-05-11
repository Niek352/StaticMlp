using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientBuildSelectionSystemTests
    {
        [Test]
        public void Update_WhenNextPressed_SwitchesPreparedBuildSnapshot()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);

            var inputState = new ClientInputState();
            inputState.Configure(
                definitionVersion: 1,
                actions: new[] { CoreInputActions.Next },
                buttonActions: new[] { true });
            inputState.BeginFrame(Vector2.zero, hasAimRay: false, aimRay: default);
            inputState.UpdateButton(0, isPressed: true);
            CW.SetResource(inputState);

            var system = new ClientBuildSelectionSystem();
            system.Update();

            Assert.That(player.Read<OwnerBuildSelection>().PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));

            ref readonly var snapshot = ref player.Read<PreparedBuildSnapshot>();
            Assert.That(snapshot.ArchetypeId, Is.EqualTo(BuildArchetypeCatalog.FireBomberId));
            Assert.That(snapshot.PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(snapshot.PreparedAbilityId, Is.EqualTo(CombatAbilityId.FireFlask));
            Assert.That(snapshot.FallbackAbilityId, Is.EqualTo(CombatAbilityId.BasicMeleeAuto));
        }

        [Test]
        public void Update_WhenBossBuildIsCommitted_DoesNotChangeSelection()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            var anchor = CW.NewEntity<Default>();
            anchor.Set(new BossBuildPreparationState
            {
                Status = BossBuildPreparationStatus.Committed
            });

            var inputState = new ClientInputState();
            inputState.Configure(
                definitionVersion: 1,
                actions: new[] { CoreInputActions.Next },
                buttonActions: new[] { true });
            inputState.BeginFrame(Vector2.zero, hasAimRay: false, aimRay: default);
            inputState.UpdateButton(0, isPressed: true);
            CW.SetResource(inputState);

            var system = new ClientBuildSelectionSystem();
            system.Update();

            Assert.That(player.Read<OwnerBuildSelection>().PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.PoisonArrowModuleId));

            ref readonly var snapshot = ref player.Read<PreparedBuildSnapshot>();
            Assert.That(snapshot.ArchetypeId, Is.EqualTo(BuildArchetypeCatalog.PoisonArcherId));
            Assert.That(snapshot.PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.PoisonArrowModuleId));
            Assert.That(snapshot.PreparedAbilityId, Is.EqualTo(CombatAbilityId.PoisonArrow));
            Assert.That(snapshot.FallbackAbilityId, Is.EqualTo(CombatAbilityId.BasicMeleeAuto));
        }
    }
}
