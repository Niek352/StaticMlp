using NUnit.Framework;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Input;
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
    }
}
