using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Client-side wrapper for OpenWorldChunkGenerationSystemBase.
    /// </summary>
    public sealed class ClientOpenWorldChunkGenerationSystem : OpenWorldChunkGenerationSystemBase<ClientCoreWT>
    {
        protected override EventReceiver<ClientCoreWT, OpenWorldChunkGenerationRequested> RegisterReceiver()
            => CW.RegisterEventReceiver<OpenWorldChunkGenerationRequested>();

        protected override void DeleteReceiver(ref EventReceiver<ClientCoreWT, OpenWorldChunkGenerationRequested> receiver)
            => CW.DeleteEventReceiver(ref receiver);

        protected override OpenWorldChunkGenerationRuntime GetRuntime()
            => CW.GetResource<OpenWorldChunkGenerationRuntime>();

        protected override void SendCompleted(in OpenWorldChunkGenerationCompleted evt)
            => CW.SendEvent(evt);
    }
}
