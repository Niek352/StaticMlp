using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ServerOpenWorldGenerationRuntimeCleanupSystem : ISystem
    {
        public void Destroy()
        {
            SW.GetResource<OpenWorldGenerationServerRuntime>().Dispose();
        }
    }
}
