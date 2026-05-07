using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiNetStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBrain, AiNetState, CharacterNetState>>().Entities())
            {
                ref readonly var brain = ref entity.Read<AiBrain>();
                ref readonly var currentNetState = ref entity.Read<AiNetState>();
                ref readonly var characterState = ref entity.Read<CharacterNetState>();

                var locomotionState = characterState.Velocity.sqrMagnitude > 0.0001f ? (byte)1 : (byte)0;
                var combatState = entity.Has<AiAttackRequest>() ? (byte)1 : (byte)0;
                if (currentNetState.CurrentTask == brain.CurrentTask
                    && currentNetState.LocomotionState == locomotionState
                    && currentNetState.CombatState == combatState)
                {
                    continue;
                }

                ref var netState = ref ReplicationMut.Mut<AiNetState>(entity);
                netState.CurrentTask = brain.CurrentTask;
                netState.LocomotionState = locomotionState;
                netState.CombatState = combatState;
            }
        }
    }
}
