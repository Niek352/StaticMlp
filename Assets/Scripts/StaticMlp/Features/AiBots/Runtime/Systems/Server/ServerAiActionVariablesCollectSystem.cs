using System;
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
            if (!SW.HasResource<AiActionCatalog>())
                throw new InvalidOperationException("AI action catalog resource is missing.");

            var catalog = SW.GetResource<AiActionCatalog>();
            if (catalog == null)
                throw new InvalidOperationException("AI action catalog resource is null.");

            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, SW.Multi<AiBlackboardEntry>, CharacterNetState>>().Entities())
                catalog.CollectVariables(entity);
        }
    }
}
