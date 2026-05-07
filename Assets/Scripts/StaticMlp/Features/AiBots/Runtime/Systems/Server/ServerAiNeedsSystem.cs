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
            foreach (var entity in SW.Query<All<ServerOwned, AiAgentTag, AiBlackboard>>().Entities())
            {
                ref var blackboard = ref entity.Mut<AiBlackboard>();
                blackboard.Hunger = MathF.Min(1f, blackboard.Hunger + Time.deltaTime * 0.015f);
                blackboard.Fear = MathF.Max(0f, blackboard.Fear - Time.deltaTime * 0.04f);
                blackboard.Health01 = MathF.Max(0f, MathF.Min(1f, blackboard.Health01));
                blackboard.WoodStorage01 = MathF.Max(0f, MathF.Min(1f, blackboard.WoodStorage01));
            }
        }
    }
}
