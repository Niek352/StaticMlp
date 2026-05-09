using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public struct UseAbilityCommand : IEvent
    {
        public CombatAbilityId AbilityId;
        public EntityGID Target;
        public uint ClientCommandId;

        public UseAbilityCommand(CombatAbilityId abilityId, EntityGID target, uint clientCommandId)
        {
            AbilityId = abilityId;
            Target = target;
            ClientCommandId = clientCommandId;
        }
    }
}
