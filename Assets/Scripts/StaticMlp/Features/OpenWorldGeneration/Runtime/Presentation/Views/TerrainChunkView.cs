using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class TerrainChunkView : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        public WorldChunkId ChunkId { get; private set; }
        public int Lod { get; private set; }
        public bool HasCollision { get; private set; }

        public void Initialize(
            MeshFilter meshFilter,
            MeshRenderer meshRenderer,
            Material material)
        {
            _meshFilter = meshFilter != null ? meshFilter : throw new ArgumentNullException(nameof(meshFilter));
            _meshRenderer = meshRenderer != null ? meshRenderer : throw new ArgumentNullException(nameof(meshRenderer));
            _meshRenderer.sharedMaterial = material != null ? material : throw new ArgumentNullException(nameof(material));
        }

        public void Apply(WorldChunkId chunkId, int lod, TerrainMeshData data, bool collision)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ChunkId = chunkId;
            Lod = lod;
            HasCollision = collision;

            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = $"Terrain Chunk {chunkId.X},{chunkId.Z} LOD{lod}"
                };
            }
            else
            {
                _mesh.Clear();
                _mesh.name = $"Terrain Chunk {chunkId.X},{chunkId.Z} LOD{lod}";
            }

            if (data.Vertices.Length > ushort.MaxValue)
                _mesh.indexFormat = IndexFormat.UInt32;

            _mesh.vertices = data.Vertices;
            _mesh.normals = data.Normals;
            _mesh.tangents = data.Tangents;
            _mesh.uv = data.Uvs;
            _mesh.colors32 = data.Colors;
            _mesh.triangles = data.Triangles;
            _mesh.bounds = data.Bounds;

            _meshFilter.sharedMesh = _mesh;

            if (collision)
            {
                _meshCollider ??= gameObject.AddComponent<MeshCollider>();
                _meshCollider.sharedMesh = _mesh;
                _meshCollider.enabled = true;
            }
            else if (_meshCollider != null)
            {
                _meshCollider.sharedMesh = null;
                _meshCollider.enabled = false;
            }
        }

        public void Dispose()
        {
            DestroyOwnedObject(gameObject);
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                DestroyOwnedObject(_mesh);
                _mesh = null;
            }
        }

        private static void DestroyOwnedObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }
}
