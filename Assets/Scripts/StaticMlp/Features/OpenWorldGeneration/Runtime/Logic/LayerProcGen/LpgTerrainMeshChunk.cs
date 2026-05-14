using Runevision.LayerProcGen;

namespace StaticMlp.Features.OpenWorldGeneration
{
    internal sealed class LpgTerrainMeshChunk : LayerChunk<LpgTerrainMeshLayer, LpgTerrainMeshChunk>
    {
        private readonly TerrainMeshData[] _meshes = new TerrainMeshData[LpgTerrainMeshLayer.LOD_COUNT];

        public TerrainMeshData GetMesh(int lod)
        {
            return _meshes[lod];
        }

        public override void Create(int level, bool destroy)
        {
            if (destroy)
            {
                _meshes[level] = null;
                return;
            }

            var settings = layer.Context.Settings;
            _meshes[level] = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(
                    new WorldChunkId(index.x, index.y),
                    settings.ChunkWorldSize,
                    level,
                    settings.BaseQuadCount,
                    settings.AddSkirts,
                    settings.SkirtDepth),
                new LpgSurfaceSampler(this));
        }
    }
}
