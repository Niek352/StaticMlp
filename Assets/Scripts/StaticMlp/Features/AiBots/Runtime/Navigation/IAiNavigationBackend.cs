using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public interface IAiNavigationBackend : IDisposable
    {
        void BeginFrame();
        void SyncAgent(in AiNavigationRuntime.AgentInput input);
        void RemoveInactiveAgents();
        bool TryGetResult(EntityGID gid, out AiNavigationRuntime.AgentResult result);
        bool TryGetDebugState(EntityGID gid, out AiNavigationRuntime.DebugState debugState);
    }
}
