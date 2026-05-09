using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientCombatAbilitySelectionSystemTests
    {
        [Test]
        public void Update_WhenNextPressed_CyclesSelectedAbility()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            player.Set(new PlayerCombatAbilityState
            {
                SelectedAbility = CombatAbilityId.BasicMeleeAuto
            });

            var inputState = new ClientInputState();
            inputState.Configure(
                definitionVersion: 1,
                actions: new[] { CoreInputActions.Next },
                buttonActions: new[] { true });
            inputState.BeginFrame(Vector2.zero, hasAimRay: false, aimRay: default);
            inputState.UpdateButton(0, isPressed: true);
            CW.SetResource(inputState);

            var system = new ClientCombatAbilitySelectionSystem();
            system.Update();

            Assert.That(player.Read<PlayerCombatAbilityState>().SelectedAbility, Is.EqualTo(CombatAbilityId.PoisonArrow));
        }
    }
}
