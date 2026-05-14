using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ClientOpenWorldTerrainStreamingSystem : ISystem
    {
        private OpenWorldTerrainStreamingConfig _config;
        private OpenWorldTerrainRuntime _runtime;
#if UNITY_EDITOR
        private float _nextDebugLogTime;
#endif

        public void Init()
        {
            _config = OpenWorldTerrainStreamingConfig.Default();
            _runtime = OpenWorldTerrainRuntime.Create(_config);
            CW.SetResource(_config);
            CW.SetResource(_runtime);
        }

        public void Update()
        {
            if (!TryGetFocusPosition(out var focusPosition))
                return;

            _runtime.StreamAround(focusPosition);
            LogDebugSnapshotIfNeeded();
        }

        public void Destroy()
        {
            if (_runtime != null)
            {
                _runtime.Dispose();
                _runtime = null;
                _config = null;
            }
        }

        private void LogDebugSnapshotIfNeeded()
        {
#if UNITY_EDITOR
            if (!_config.LogDebugStreaming)
                return;
            if (Time.unscaledTime < _nextDebugLogTime)
                return;

            _nextDebugLogTime = Time.unscaledTime + _config.DebugLogIntervalSeconds;
            var snapshot = _runtime.CreateDebugSnapshot();
            Debug.Log(
                $"OpenWorld terrain seed={snapshot.Seed.Value} focus={snapshot.FocusChunk} loaded={snapshot.LoadedChunkCount} " +
                $"colliders={snapshot.ColliderChunkCount} lod0={snapshot.CountLod(0)} lod1={snapshot.CountLod(1)} " +
                $"lod2={snapshot.CountLod(2)} lod3={snapshot.CountLod(3)}");
#endif
        }

        private static bool TryGetFocusPosition(out Vector3 position)
        {
            foreach (var entity in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = entity.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }
    }
}
