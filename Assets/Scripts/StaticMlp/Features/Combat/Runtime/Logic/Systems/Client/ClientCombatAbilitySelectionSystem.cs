using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatAbilitySelectionSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                if (!player.Has<PlayerCombatAbilityState>())
                    player.Set(new PlayerCombatAbilityState { SelectedAbility = CombatAbilityId.BasicMeleeAuto });

                ref var state = ref player.Mut<PlayerCombatAbilityState>();
                if (inputState.WasPressed(CoreInputActions.Next))
                    state.SelectedAbility = Next(state.SelectedAbility);
                else if (inputState.WasPressed(CoreInputActions.Previous))
                    state.SelectedAbility = Previous(state.SelectedAbility);
            }
        }

        private static CombatAbilityId Next(CombatAbilityId current)
        {
            switch (current)
            {
                case CombatAbilityId.BasicMeleeAuto:
                    return CombatAbilityId.PoisonArrow;
                case CombatAbilityId.PoisonArrow:
                    return CombatAbilityId.FireFlask;
                default:
                    return CombatAbilityId.BasicMeleeAuto;
            }
        }

        private static CombatAbilityId Previous(CombatAbilityId current)
        {
            switch (current)
            {
                case CombatAbilityId.FireFlask:
                    return CombatAbilityId.PoisonArrow;
                case CombatAbilityId.PoisonArrow:
                    return CombatAbilityId.BasicMeleeAuto;
                default:
                    return CombatAbilityId.FireFlask;
            }
        }
    }
}
