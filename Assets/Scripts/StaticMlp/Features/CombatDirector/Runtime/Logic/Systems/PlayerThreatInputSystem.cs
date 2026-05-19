using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class PlayerThreatInputSystem : ISystem
    {
        private const float ATTACK_NOISE_PER_SHOT = 1f;
        private const float HARVEST_NOISE_PER_ACTION = 1f;
        private const float TIME_IN_CELL_NOISE_PER_SECOND = 0.5f;
        private const float MAX_SPAWN_SOURCE_PROXIMITY_NOISE = 1f;

        private readonly Dictionary<EntityGID, float> _frameNoiseByPlayer = new();
        private EventReceiver<ServerWT, NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>> _harvestRequests;

        public void Init()
        {
            _harvestRequests = SW.RegisterEventReceiver<NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _harvestRequests);
        }

        public void Update()
        {
            EnsurePlayerThreatState();

            _frameNoiseByPlayer.Clear();
            AccumulateHarvestNoise();

            var cell = ReadSingleCell();
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, ServerCombatAttackState, PlayerThreatInputState, PlayerNoise, CarriedLootValue>>().Entities())
            {
                ref var threatState = ref player.Mut<PlayerThreatInputState>();
                ref readonly var attackState = ref player.Read<ServerCombatAttackState>();
                if (attackState.LastAcceptedShotSequence < threatState.LastAcceptedShotSequence)
                {
                    throw new InvalidOperationException(
                        $"Player attack sequence regressed from {threatState.LastAcceptedShotSequence} to {attackState.LastAcceptedShotSequence}.");
                }

                var attackCountDelta = attackState.LastAcceptedShotSequence - threatState.LastAcceptedShotSequence;
                if (attackCountDelta > 0)
                    AddNoise(player.GID, attackCountDelta * ATTACK_NOISE_PER_SHOT);

                threatState.LastAcceptedShotSequence = attackState.LastAcceptedShotSequence;

                ref var carriedLootValue = ref player.Mut<CarriedLootValue>();
                carriedLootValue.Value = ComputeCarriedLootValue(player);

                var position = ToFloat3(player.Read<CharacterNetState>().Position);
                var noise = GetActionNoise(player.GID);
                noise += ComputeSpawnSourceProximityNoise(position, in cell);
                if (IsInsideCell(position, in cell))
                    noise += deltaTime * TIME_IN_CELL_NOISE_PER_SECOND;

                if (noise < 0f)
                    throw new InvalidOperationException("Player noise must be non-negative.");

                ref var playerNoise = ref player.Mut<PlayerNoise>();
                playerNoise.Value = noise;
            }
        }

        private void EnsurePlayerThreatState()
        {
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, ServerCombatAttackState>>().Entities())
            {
                if (!player.Has<PlayerThreatInputState>())
                {
                    player.Set(new PlayerThreatInputState
                    {
                        LastAcceptedShotSequence = player.Read<ServerCombatAttackState>().LastAcceptedShotSequence
                    });
                }

                if (!player.Has<PlayerNoise>())
                    player.Set(default(PlayerNoise));

                if (!player.Has<CarriedLootValue>())
                    player.Set(default(CarriedLootValue));
            }
        }

        private void AccumulateHarvestNoise()
        {
            foreach (var evt in _harvestRequests)
            {
                if (!ServerPeerPlayers.TryGetPlayer(evt.Value.SourcePeer, out var player))
                    continue;

                AddNoise(player.GID, HARVEST_NOISE_PER_ACTION);
            }
        }

        private static CombatCell ReadSingleCell()
        {
            var found = false;
            CombatCell cell = default;

            foreach (var entity in SW.Query<All<CombatCell>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single combat cell.");

                cell = entity.Read<CombatCell>();
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat cell entity must exist before threat input updates.");

            return cell;
        }

        private static float ComputeCarriedLootValue(SW.Entity player)
        {
            if (!player.Has<ResourcesInventory>())
                return 0f;

            ref readonly var inventory = ref player.Read<ResourcesInventory>();
            return inventory.Wood + inventory.Stone;
        }

        private static float ComputeSpawnSourceProximityNoise(float3 playerPosition, in CombatCell cell)
        {
            var bestContribution = 0f;
            foreach (var sourceEntity in SW.Query<All<SpawnSource>>().Entities())
            {
                ref readonly var source = ref sourceEntity.Read<SpawnSource>();
                if (!source.IsActive)
                    continue;

                var influenceRadius = math.max(source.Radius, cell.Radius);
                var distance = math.distance(playerPosition, source.Position);
                var contribution = MAX_SPAWN_SOURCE_PROXIMITY_NOISE * math.saturate(1f - distance / influenceRadius);
                if (contribution > bestContribution)
                    bestContribution = contribution;
            }

            return bestContribution;
        }

        private static bool IsInsideCell(float3 position, in CombatCell cell)
        {
            return math.distancesq(position, cell.Center) <= cell.Radius * cell.Radius;
        }

        private float GetActionNoise(EntityGID playerGid)
        {
            return _frameNoiseByPlayer.TryGetValue(playerGid, out var noise)
                ? noise
                : 0f;
        }

        private void AddNoise(EntityGID playerGid, float noise)
        {
            if (_frameNoiseByPlayer.TryGetValue(playerGid, out var current))
            {
                _frameNoiseByPlayer[playerGid] = current + noise;
                return;
            }

            _frameNoiseByPlayer.Add(playerGid, noise);
        }

        private static float3 ToFloat3(UnityEngine.Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }
    }
}
