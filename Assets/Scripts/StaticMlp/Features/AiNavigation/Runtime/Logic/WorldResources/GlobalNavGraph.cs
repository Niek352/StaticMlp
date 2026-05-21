using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class GlobalNavGraph : IResource
    {
        private readonly List<int2> _neighbors = new(8);

        public bool TryFindPath(int2 fromChunk, int2 toChunk, ChunkNavSourceRegistry registry, List<int2> outPath)
        {
            outPath.Clear();

            if (fromChunk.Equals(toChunk))
            {
                outPath.Add(toChunk);
                return true;
            }

            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            return AStar(fromChunk, toChunk, registry, outPath);
        }

        private bool AStar(int2 start, int2 goal, ChunkNavSourceRegistry registry, List<int2> outPath)
        {
            var openSet = new PriorityQueue<int2, float>();
            var cameFrom = new Dictionary<int2, int2>();
            var gScore = new Dictionary<int2, float>();
            var fScore = new Dictionary<int2, float>();

            openSet.Enqueue(start, 0f);
            gScore[start] = 0f;
            fScore[start] = Heuristic(start, goal);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Equals(goal))
                {
                    ReconstructPath(cameFrom, current, outPath);
                    return true;
                }

                GetNeighbors(current, registry, _neighbors);
                for (var i = 0; i < _neighbors.Count; i++)
                {
                    var neighbor = _neighbors[i];
                    var tentativeG = gScore[current] + Heuristic(current, neighbor);

                    if (!gScore.TryGetValue(neighbor, out var neighborG) || tentativeG < neighborG)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }

                _neighbors.Clear();
            }

            return false;
        }

        private static void ReconstructPath(Dictionary<int2, int2> cameFrom, int2 current, List<int2> outPath)
        {
            outPath.Add(current);
            while (cameFrom.TryGetValue(current, out var previous))
            {
                outPath.Add(previous);
                current = previous;
            }

            outPath.Reverse();
        }

        private static float Heuristic(int2 a, int2 b)
        {
            return math.distance(new float2(a.x, a.y), new float2(b.x, b.y));
        }

        private static void GetNeighbors(int2 coord, ChunkNavSourceRegistry registry, List<int2> neighbors)
        {
            // 4-directional for simplicity; only include registered chunks
            TryAddNeighbor(coord.x - 1, coord.y, registry, neighbors);
            TryAddNeighbor(coord.x + 1, coord.y, registry, neighbors);
            TryAddNeighbor(coord.x, coord.y - 1, registry, neighbors);
            TryAddNeighbor(coord.x, coord.y + 1, registry, neighbors);
        }

        private static void TryAddNeighbor(int x, int z, ChunkNavSourceRegistry registry, List<int2> neighbors)
        {
            var chunkId = new WorldChunkId(x, z);
            if (registry.HasChunk(chunkId))
                neighbors.Add(new int2(x, z));
        }

        private sealed class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
        {
            private readonly List<(TElement Element, TPriority Priority)> _heap = new();

            public int Count => _heap.Count;

            public void Enqueue(TElement element, TPriority priority)
            {
                _heap.Add((element, priority));
                SiftUp(_heap.Count - 1);
            }

            public TElement Dequeue()
            {
                if (_heap.Count == 0)
                    throw new InvalidOperationException("Priority queue is empty.");

                var result = _heap[0].Element;
                _heap[0] = _heap[_heap.Count - 1];
                _heap.RemoveAt(_heap.Count - 1);

                if (_heap.Count > 0)
                    SiftDown(0);

                return result;
            }

            private void SiftUp(int index)
            {
                while (index > 0)
                {
                    var parent = (index - 1) / 2;
                    if (_heap[parent].Priority.CompareTo(_heap[index].Priority) <= 0)
                        break;

                    (_heap[parent], _heap[index]) = (_heap[index], _heap[parent]);
                    index = parent;
                }
            }

            private void SiftDown(int index)
            {
                while (true)
                {
                    var left = index * 2 + 1;
                    var right = index * 2 + 2;
                    var smallest = index;

                    if (left < _heap.Count && _heap[left].Priority.CompareTo(_heap[smallest].Priority) < 0)
                        smallest = left;
                    if (right < _heap.Count && _heap[right].Priority.CompareTo(_heap[smallest].Priority) < 0)
                        smallest = right;

                    if (smallest == index)
                        break;

                    (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
                    index = smallest;
                }
            }
        }
    }
}
