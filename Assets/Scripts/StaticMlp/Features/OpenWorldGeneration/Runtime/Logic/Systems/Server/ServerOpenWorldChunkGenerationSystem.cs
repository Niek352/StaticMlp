using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Server-side wrapper for OpenWorldChunkGenerationSystemBase.
    /// </summary>
    public sealed class ServerOpenWorldChunkGenerationSystem : OpenWorldChunkGenerationSystemBase<ServerWT>
    {
        protected override EventReceiver<ServerWT, OpenWorldChunkGenerationRequested> RegisterReceiver()
            => SW.RegisterEventReceiver<OpenWorldChunkGenerationRequested>();

        protected override void DeleteReceiver(ref EventReceiver<ServerWT, OpenWorldChunkGenerationRequested> receiver)
            => SW.DeleteEventReceiver(ref receiver);

        protected override OpenWorldChunkGenerationRuntime GetRuntime()
            => SW.GetResource<OpenWorldChunkGenerationRuntime>();

        protected override void SendCompleted(in OpenWorldChunkGenerationCompleted evt)
            => SW.SendEvent(evt);
    }
}
