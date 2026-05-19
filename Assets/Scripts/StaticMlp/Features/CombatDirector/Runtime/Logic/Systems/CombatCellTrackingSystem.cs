using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class CombatCellTrackingSystem : ISystem
    {
        private readonly List<float3> _playerPositions = new();
        private readonly List<int> _candidateGroup = new();
        private readonly List<int> _bestGroup = new();
        private bool[] _visited = Array.Empty<bool>();
        private bool _cellInitialized;

        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            var cellEntity = EnsureCellEntity(config.CellRadius);
            var center = ComputeCenter(config.CellRadius);

            if (!math.all(math.isfinite(center)))
                throw new InvalidOperationException("Combat cell center must be finite.");

            ref var combatCell = ref cellEntity.Mut<CombatCell>();
            combatCell.CellId = 0;
            combatCell.Center = center;
            combatCell.Radius = config.CellRadius;
        }

        private SW.Entity EnsureCellEntity(float cellRadius)
        {
            var found = false;
            SW.Entity cellEntity = default;

            foreach (var entity in SW.Query<All<CombatCell>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single combat cell.");

                cellEntity = entity;
                found = true;
            }

            if (found)
            {
                _cellInitialized = true;
                return cellEntity;
            }

            if (_cellInitialized)
                throw new InvalidOperationException("Combat cell entity was deleted after initialization.");

            cellEntity = SW.NewEntity<Default>();
            cellEntity.Set(new CombatCell
            {
                CellId = 0,
                Center = float3.zero,
                Radius = cellRadius
            });
            _cellInitialized = true;
            return cellEntity;
        }

        private float3 ComputeCenter(float cellRadius)
        {
            _playerPositions.Clear();

            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
            {
                var position = player.Read<CharacterNetState>().Position;
                _playerPositions.Add(new float3(position.x, position.y, position.z));
            }

            if (_playerPositions.Count == 0)
                return float3.zero;

            if (_playerPositions.Count == 1)
                return _playerPositions[0];

            return ComputeActiveGroupCenter(cellRadius);
        }

        private float3 ComputeActiveGroupCenter(float cellRadius)
        {
            EnsureVisitedCapacity(_playerPositions.Count);

            _bestGroup.Clear();
            var linkDistanceSq = cellRadius * cellRadius * 4f;
            for (var seedIndex = 0; seedIndex < _playerPositions.Count; seedIndex++)
            {
                CollectConnectedGroup(seedIndex, linkDistanceSq);
                if (_candidateGroup.Count <= _bestGroup.Count)
                    continue;

                _bestGroup.Clear();
                _bestGroup.AddRange(_candidateGroup);
            }

            return AverageGroupCenter(_bestGroup);
        }

        private void CollectConnectedGroup(int seedIndex, float linkDistanceSq)
        {
            Array.Clear(_visited, 0, _playerPositions.Count);
            _candidateGroup.Clear();
            _candidateGroup.Add(seedIndex);
            _visited[seedIndex] = true;

            for (var currentIndex = 0; currentIndex < _candidateGroup.Count; currentIndex++)
            {
                var currentPosition = _playerPositions[_candidateGroup[currentIndex]];
                for (var otherIndex = 0; otherIndex < _playerPositions.Count; otherIndex++)
                {
                    if (_visited[otherIndex])
                        continue;

                    if (math.distancesq(currentPosition, _playerPositions[otherIndex]) > linkDistanceSq)
                        continue;

                    _visited[otherIndex] = true;
                    _candidateGroup.Add(otherIndex);
                }
            }
        }

        private float3 AverageGroupCenter(List<int> groupIndices)
        {
            var sum = float3.zero;
            for (var i = 0; i < groupIndices.Count; i++)
                sum += _playerPositions[groupIndices[i]];

            return sum / groupIndices.Count;
        }

        private void EnsureVisitedCapacity(int count)
        {
            if (_visited.Length < count)
                _visited = new bool[count];
        }
    }
}
