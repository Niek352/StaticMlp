using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiActionVariablesCollectSystem : ISystem
    {
        public void Update()
        {
            var catalog = SW.GetResource<AiActionCatalog>();

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, SW.Multi<AiBlackboardEntry>, CharacterNetState>>().Entities())
                catalog.CollectVariables(entity);
        }
    }
}
