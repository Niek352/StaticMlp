using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat {
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public struct UseAbilityCommand : IEvent {
        public const ushort NETWORK_EVENT_ID = 57022;

        public CombatAbilityId AbilityId;
        public CombatTargetRef Target;
        public uint ClientCommandId;

        public UseAbilityCommand(CombatAbilityId abilityId, CombatTargetRef target, uint clientCommandId) {
            AbilityId = abilityId;
            Target = target;
            ClientCommandId = clientCommandId;
        }
    }
}
