using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiNeedsSystem : ISystem
    {
        public void Update()
        {
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, Health, SW.Multi<AiBlackboardEntry>>>().Entities())
            {
                var hunger = MathF.Min(1f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Hunger) + deltaTime * 0.015f);
                var fear = MathF.Max(0f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.Fear) - deltaTime * 0.04f);
                ref readonly var health = ref entity.Read<Health>();
                var health01 = health.Max <= 0f
                    ? 0f
                    : MathF.Max(0f, MathF.Min(1f, health.Current / health.Max));
                var woodStorage01 = MathF.Max(0f, MathF.Min(1f, AiBlackboardAccess.GetFloat(entity, AiCoreVariableIds.WoodStorage01)));

                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Hunger, hunger);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Fear, fear);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.Health01, health01);
                AiBlackboardAccess.SetFloat(entity, AiCoreVariableIds.WoodStorage01, woodStorage01);
            }
        }
    }
}
