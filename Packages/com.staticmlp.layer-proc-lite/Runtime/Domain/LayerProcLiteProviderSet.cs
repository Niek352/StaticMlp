using System;
using System.Collections.Generic;

namespace StaticMlp.LayerProcLite
{
    public sealed class LayerProcLiteProviderSet
    {
        public static readonly LayerProcLiteProviderSet Empty = new(Array.Empty<ProviderGroup>());

        private readonly ProviderGroup[] _groups;

        internal LayerProcLiteProviderSet(ProviderGroup[] groups)
        {
            _groups = groups;
        }

        public LayerProcLiteProviderChunk<TData>[] GetOverlapping<TData>(
            LayerProcLiteLayerId layerId,
            int level,
            LayerProcLiteWorldBounds bounds)
            where TData : class, ILayerProcLiteChunkData
        {
            var groupIndex = FindGroup(layerId, level);
            if (groupIndex < 0)
                throw new InvalidOperationException($"Provider layer {layerId.Value} level {level} was not declared for this chunk.");

            var group = _groups[groupIndex];
            if (!group.DeclaredBounds.Contains(bounds))
                throw new InvalidOperationException(
                    $"Provider layer {layerId.Value} level {level} requested bounds outside declared dependency padding.");

            var results = new List<LayerProcLiteProviderChunk<TData>>(group.Chunks.Length);
            for (var i = 0; i < group.Chunks.Length; i++)
            {
                var chunk = group.Chunks[i];
                if (!chunk.Bounds.Overlaps(bounds))
                    continue;

                if (chunk.Data is not TData data)
                    throw new InvalidOperationException($"Provider layer {layerId.Value} level {level} data is not {typeof(TData).Name}.");

                results.Add(new LayerProcLiteProviderChunk<TData>(chunk.Key, chunk.Bounds, data, chunk.Handle));
            }

            return results.ToArray();
        }

        public TData GetSingleOverlapping<TData>(
            LayerProcLiteLayerId layerId,
            int level,
            LayerProcLiteWorldBounds bounds)
            where TData : class, ILayerProcLiteChunkData
        {
            var chunks = GetOverlapping<TData>(layerId, level, bounds);
            if (chunks.Length != 1)
                throw new InvalidOperationException($"Provider layer {layerId.Value} level {level} expected exactly one overlapping chunk, got {chunks.Length}.");

            return chunks[0].Data;
        }

        private int FindGroup(LayerProcLiteLayerId layerId, int level)
        {
            for (var i = 0; i < _groups.Length; i++)
            {
                if (_groups[i].LayerId == layerId && _groups[i].Level == level)
                    return i;
            }

            return -1;
        }

        internal readonly struct ProviderGroup
        {
            public ProviderGroup(
                LayerProcLiteLayerId layerId,
                int level,
                LayerProcLiteWorldBounds declaredBounds,
                LayerProcLiteProviderChunkInfo[] chunks)
            {
                LayerId = layerId;
                Level = level;
                DeclaredBounds = declaredBounds;
                Chunks = chunks;
            }

            public readonly LayerProcLiteLayerId LayerId;
            public readonly int Level;
            public readonly LayerProcLiteWorldBounds DeclaredBounds;
            public readonly LayerProcLiteProviderChunkInfo[] Chunks;
        }
    }
}
