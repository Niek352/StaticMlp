using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceHarvestCommandSystem : ISystem
    {
        private const float HIT_POINT_QUANTIZATION = 0.01f;
        private const float HARVEST_INTERACTION_RANGE = 4f;
        private const float HARVEST_COOLDOWN_SECONDS = 0.4f;
        private const ushort DEFAULT_TOOL_ID = 0;

        private EventReceiver<ServerWT, NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            var placementIndex = SW.GetResource<OpenWorldPlacementIndexStore>();
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            var simulationTime = SW.GetResource<SimulationTime>();

            foreach (var request in _requests)
                Handle(in request.Value, placementIndex, overlayStore, simulationTime);
        }

        private static void Handle(
            in NetworkEventFromClient<TryHarvestOpenWorldResourceCommand> request,
            OpenWorldPlacementIndexStore placementIndex,
            OpenWorldChunkOverlayStore overlayStore,
            SimulationTime simulationTime)
        {
            var command = request.Value;
            if (command.ToolId != DEFAULT_TOOL_ID)
                return;

            if (!ServerPeerPlayers.TryGetPlayer(request.SourcePeer, out var player))
                return;

            if (!placementIndex.TryGetPlacement(command.PlacementId, out var placement))
                return;

            if (!overlayStore.TryGetChunkId(command.PlacementId, out var chunkId))
                throw new InvalidOperationException($"Cannot harvest resource placement {command.PlacementId}: placement is not registered in the authority overlay store.");
            if (chunkId != placement.ChunkId)
                throw new InvalidOperationException($"Cannot harvest resource placement {command.PlacementId}: placement index chunk {placement.ChunkId} does not match overlay chunk {chunkId}.");

            var hitPoint = ToWorldPosition(in command);
            if (!IsFinite(hitPoint))
                return;

            var playerPosition = player.Read<CharacterNetState>().Position;
            if (!IsInRange(playerPosition, placement.Position)
                || !IsInRange(playerPosition, hitPoint))
                return;

            if (player.Has<OpenWorldResourceHarvestCooldownState>()
                && simulationTime.ServerTick < player.Read<OpenWorldResourceHarvestCooldownState>().NextHarvestTick)
                return;

            var currentState = overlayStore.GetEffectiveResourceState(placement);
            if (!IsActive(currentState))
                return;

            var nextState = OpenWorldResourceNodeRules.ApplyHit(
                placement,
                currentState,
                out var harvestedResource,
                out var wasDepleted);

            if (!overlayStore.TryApplyResourceState(chunkId, nextState))
                throw new InvalidOperationException($"Resource harvest for placement {command.PlacementId} did not change overlay state.");

            SetCooldown(player, simulationTime);
            var authoritativeHitPoint = QuantizeWorldPosition(placement.Position);
            SW.SendEvent(new OpenWorldResourceHarvestedEvent
            {
                SourcePlayer = player.GID,
                PlacementId = command.PlacementId,
                Resource = harvestedResource,
                HitPointXQ = authoritativeHitPoint.x,
                HitPointYQ = authoritativeHitPoint.y,
                HitPointZQ = authoritativeHitPoint.z,
                WasDepleted = wasDepleted
            });

            if (wasDepleted)
            {
                SW.SendEvent(new OpenWorldResourceDepletionHazardEvent
                {
                    SourcePlayer = player.GID,
                    PlacementId = placement.PlacementId,
                    KindId = placement.KindId,
                    Origin = placement.Position
                });
            }
        }

        private static bool IsActive(OpenWorldResourceOverlayState state)
        {
            const OpenWorldResourceOverlayFlags inactive =
                OpenWorldResourceOverlayFlags.Depleted
                | OpenWorldResourceOverlayFlags.Hidden
                | OpenWorldResourceOverlayFlags.Replaced;

            return state.PlacementId != 0L
                   && state.RemainingAmount > 0
                   && (state.Flags & inactive) == 0;
        }

        private static Vector3 ToWorldPosition(in TryHarvestOpenWorldResourceCommand command)
        {
            return new Vector3(
                command.HitPointXQ * HIT_POINT_QUANTIZATION,
                command.HitPointYQ * HIT_POINT_QUANTIZATION,
                command.HitPointZQ * HIT_POINT_QUANTIZATION);
        }

        private static (int x, int y, int z) QuantizeWorldPosition(Vector3 position)
        {
            return (
                Mathf.RoundToInt(position.x / HIT_POINT_QUANTIZATION),
                Mathf.RoundToInt(position.y / HIT_POINT_QUANTIZATION),
                Mathf.RoundToInt(position.z / HIT_POINT_QUANTIZATION));
        }

        private static void SetCooldown(SW.Entity player, SimulationTime simulationTime)
        {
            var deadline = simulationTime.DeadlineAfter(HARVEST_COOLDOWN_SECONDS);
            if (player.Has<OpenWorldResourceHarvestCooldownState>())
            {
                player.Mut<OpenWorldResourceHarvestCooldownState>().NextHarvestTick = deadline;
                return;
            }

            player.Set(new OpenWorldResourceHarvestCooldownState
            {
                NextHarvestTick = deadline
            });
        }

        private static bool IsInRange(Vector3 sourcePosition, Vector3 targetPosition)
        {
            return (sourcePosition - targetPosition).sqrMagnitude <= HARVEST_INTERACTION_RANGE * HARVEST_INTERACTION_RANGE;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
