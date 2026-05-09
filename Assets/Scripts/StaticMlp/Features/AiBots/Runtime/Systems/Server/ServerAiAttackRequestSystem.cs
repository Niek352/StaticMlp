using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiAttackRequestSystem : ISystem
    {
        public void Update()
        {
            foreach (var attacker in SW.Query<All<ServerOwned, AiAgentTag, AiAttackRequest, ServerCombatAttackState>>().Entities())
            {
                ref readonly var request = ref attacker.Read<AiAttackRequest>();
                ServerReceiveCombatCommandsSystem.CreateRequest(
                    attacker.GID,
                    CombatAbilityId.BasicMeleeAuto,
                    request.Target,
                    0);
            }
        }
    }
}
