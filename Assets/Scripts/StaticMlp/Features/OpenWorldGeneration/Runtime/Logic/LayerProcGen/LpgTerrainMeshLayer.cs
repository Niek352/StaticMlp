using Runevision.Common;
using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgTerrainMeshLayer : ChunkBasedDataLayer<LpgTerrainMeshLayer, LpgTerrainMeshChunk>
    {
        public const int LOD_COUNT = 4;

        public override int chunkW => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;
        public override int chunkH => LayerProcGenWorldGenerationService.Context.Settings.ChunkWorldSize;

        public LayerProcGenWorldContext Context => LayerProcGenWorldGenerationService.Context;

        public override int GetLevelCount()
        {
            return LOD_COUNT;
        }

        public LpgTerrainMeshLayer()
        {
            AddLayerDependency(new LayerDependency(LpgSurfaceLayer.instance, 1));
        }

        public bool TryGetGeneratedChunk(
            WorldChunkId chunkId,
            int lod,
            out TerrainMeshData mesh,
            out ResourcePlacement[] resourcePlacements,
            out SpawnPlacement[] spawnPlacements)
        {
            var index = new Point(chunkId.X, chunkId.Z);
            if (TryGetChunk(index, out var chunk, lod))
            {
                mesh = chunk.GetMesh(lod);
                resourcePlacements = chunk.GetResourcePlacements(lod);
                spawnPlacements = chunk.GetSpawnPlacements(lod);
                return mesh != null && resourcePlacements != null && spawnPlacements != null;
            }

            mesh = null;
            resourcePlacements = null;
            spawnPlacements = null;
            return false;
        }
    }
}
