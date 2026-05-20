using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class PlayerThreatInputSystem : ISystem
    {
        private const float ATTACK_NOISE_PER_SHOT = 1f;
        private const float HARVEST_NOISE_PER_ACTION = 1f;

        private readonly Dictionary<EntityGID, float> _frameNoiseByPlayer = new();
        private readonly Dictionary<EntityGID, float> _frameCombatByPlayer = new();
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
            PreparePlayerThreatState();

            _frameNoiseByPlayer.Clear();
            _frameCombatByPlayer.Clear();
            AccumulateHarvestNoise();

            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, ServerCombatAttackState, PlayerThreatInputState, PlayerNoise, PlayerCombatAttention, CarriedLootValue>>().Entities())
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
                    AddCombat(player.GID, attackCountDelta * ATTACK_NOISE_PER_SHOT);

                threatState.LastAcceptedShotSequence = attackState.LastAcceptedShotSequence;

                ref var carriedLootValue = ref player.Mut<CarriedLootValue>();
                carriedLootValue.Value = ComputeCarriedLootValue(player);

                var noise = GetActionNoise(player.GID);
                if (noise < 0f)
                    throw new InvalidOperationException("Player noise must be non-negative.");

                ref var playerNoise = ref player.Mut<PlayerNoise>();
                playerNoise.Value = noise;

                var combat = GetActionCombat(player.GID);
                if (combat < 0f)
                    throw new InvalidOperationException("Player combat attention must be non-negative.");

                ref var playerCombat = ref player.Mut<PlayerCombatAttention>();
                playerCombat.Value = combat;
            }
        }

        private void PreparePlayerThreatState()
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

                if (!player.Has<PlayerCombatAttention>())
                    player.Set(default(PlayerCombatAttention));

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

        private static float ComputeCarriedLootValue(SW.Entity player)
        {
            if (!player.Has<ResourcesInventory>())
                return 0f;

            ref readonly var inventory = ref player.Read<ResourcesInventory>();
            return inventory.Wood + inventory.Stone;
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

        private float GetActionCombat(EntityGID playerGid)
        {
            return _frameCombatByPlayer.TryGetValue(playerGid, out var combat)
                ? combat
                : 0f;
        }

        private void AddCombat(EntityGID playerGid, float combat)
        {
            if (_frameCombatByPlayer.TryGetValue(playerGid, out var current))
            {
                _frameCombatByPlayer[playerGid] = current + combat;
                return;
            }

            _frameCombatByPlayer.Add(playerGid, combat);
        }
    }
}
