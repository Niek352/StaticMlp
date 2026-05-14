using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgTerrainMeshChunk : LayerChunk<LpgTerrainMeshLayer, LpgTerrainMeshChunk>
    {
        private readonly TerrainMeshData[] _meshes = new TerrainMeshData[LpgTerrainMeshLayer.LOD_COUNT];
        private readonly ResourcePlacement[][] _resourcePlacements = new ResourcePlacement[LpgTerrainMeshLayer.LOD_COUNT][];
        private readonly SpawnPlacement[][] _spawnPlacements = new SpawnPlacement[LpgTerrainMeshLayer.LOD_COUNT][];

        public TerrainMeshData GetMesh(int lod)
        {
            return _meshes[lod];
        }

        public ResourcePlacement[] GetResourcePlacements(int lod)
        {
            return _resourcePlacements[lod];
        }

        public SpawnPlacement[] GetSpawnPlacements(int lod)
        {
            return _spawnPlacements[lod];
        }

        public override void Create(int level, bool destroy)
        {
            if (destroy)
            {
                _meshes[level] = null;
                _resourcePlacements[level] = null;
                _spawnPlacements[level] = null;
                return;
            }

            var settings = layer.Context.Settings;
            var request = new WorldGenerationRequest(
                settings.Seed,
                settings.Bounds,
                settings.ChunkWorldSize,
                settings.BaseQuadCount,
                level,
                settings.AddSkirts,
                settings.SkirtDepth);
            var chunkId = new WorldChunkId(index.x, index.y);
            var sampler = new LpgSurfaceSampler(this);
            _meshes[level] = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(
                    chunkId,
                    settings.ChunkWorldSize,
                    level,
                    settings.BaseQuadCount,
                    settings.AddSkirts,
                    settings.SkirtDepth),
                sampler);
            _resourcePlacements[level] = OpenWorldPlacementGenerator.GenerateResourcePlacements(chunkId, request, sampler);
            _spawnPlacements[level] = OpenWorldPlacementGenerator.GenerateSpawnPlacements(chunkId, request, sampler);
        }
    }
}
