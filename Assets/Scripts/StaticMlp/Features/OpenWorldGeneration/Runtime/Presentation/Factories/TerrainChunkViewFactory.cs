using StaticMlp.Game.Presentation;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class TerrainChunkViewFactory
    {
        private readonly Transform _root;
        private readonly Material _material;

        public TerrainChunkViewFactory(Transform root, Color materialColor)
        {
            _root = root;
            _material = RuntimeVisualMaterial.Create(materialColor);
        }

        public TerrainChunkView Create(WorldChunkId chunkId, float chunkWorldSize)
        {
            var gameObject = new GameObject($"Terrain Chunk {chunkId.X},{chunkId.Z}");
            gameObject.transform.SetParent(_root, false);
            gameObject.transform.position = chunkId.GetWorldOrigin(chunkWorldSize);

            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            var view = gameObject.AddComponent<TerrainChunkView>();
            view.Initialize(meshFilter, meshRenderer, _material);
            return view;
        }

        public void Dispose()
        {
            if (Application.isPlaying)
                Object.Destroy(_material);
            else
                Object.DestroyImmediate(_material);
        }
    }
}
