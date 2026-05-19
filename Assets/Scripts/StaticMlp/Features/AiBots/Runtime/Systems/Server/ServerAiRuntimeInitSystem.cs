using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine.AI;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiRuntimeInitSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(AiActionCatalog.Discover(new AiTaskExecutionTransitions()));
            SW.SetResource(AiNavigationRuntime.CreateDefault());
        }
    }
}
