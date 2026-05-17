using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Applies generated terrain mesh to TerrainChunkView on the client.
    /// </summary>
    public sealed class ClientOpenWorldTerrainApplySystem : ISystem
    {
        private EventReceiver<ClientCoreWT, OpenWorldChunkGenerationCompleted> _completed;
        private OpenWorldTerrainRuntime _runtime;

        public void Init()
        {
            _completed = CW.RegisterEventReceiver<OpenWorldChunkGenerationCompleted>();
            _runtime = CW.GetResource<OpenWorldTerrainRuntime>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _completed);
        }

        public void Update()
        {
            foreach (var evt in _completed)
            {
                if (evt.Value.TerrainMesh != null)
                    _runtime.ApplyChunkMesh(evt.Value.ChunkId, evt.Value.Lod, evt.Value.TerrainMesh);
            }
        }
    }
}
