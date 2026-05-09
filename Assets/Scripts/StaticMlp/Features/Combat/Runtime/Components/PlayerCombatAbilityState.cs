using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct PlayerCombatAbilityState : IComponent
    {
        public CombatAbilityId SelectedAbility;
    }
}
