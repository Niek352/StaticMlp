using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiNeedsSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, SW.Multi<AiBlackboardEntry>>>().Entities())
            {
                var hunger = MathF.Min(1f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Hunger) + Time.deltaTime * 0.015f);
                var fear = MathF.Max(0f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Fear) - Time.deltaTime * 0.04f);
                var health01 = MathF.Max(0f, MathF.Min(1f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Health01)));
                var woodStorage01 = MathF.Max(0f, MathF.Min(1f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.WoodStorage01)));

                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Hunger, hunger);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Fear, fear);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Health01, health01);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.WoodStorage01, woodStorage01);
            }
        }
    }
}
