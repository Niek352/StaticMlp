using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Statuses
{
    public static class StatusEntitySpawns
    {
        public static EntityGID SpawnPoison(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.POISON,
                entity =>
                {
                    entity.Set<PoisonStatus>();
                    var intervalTicks = simulationTime.SecondsToTicks(spec.TickInterval);
                    entity.Set(new StatusTickState
                    {
                        IntervalTicks = intervalTicks,
                        NextTick = intervalTicks == 0 ? 0 : simulationTime.ServerTick + intervalTicks,
                    });
                });
        }

        public static EntityGID SpawnBurning(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.BURNING,
                entity =>
                {
                    entity.Set<BurningStatus>();
                    var intervalTicks = simulationTime.SecondsToTicks(spec.TickInterval);
                    entity.Set(new StatusTickState
                    {
                        IntervalTicks = intervalTicks,
                        NextTick = intervalTicks == 0 ? 0 : simulationTime.ServerTick + intervalTicks,
                    });
                });
        }

        public static EntityGID SpawnOiled(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.OILED,
                entity => entity.Set<OiledStatus>());
        }

        private static EntityGID Spawn(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime,
            ushort networkArchetypeId,
            Action<SW.Entity> initialize)
        {
            var owner = target.Has<NetworkIdentity>()
                ? target.Read<NetworkIdentity>().Owner
                : default;

            return NetworkEntitySpawner.SpawnServerEntity<StatusNetworkEntity>(
                owner,
                NetworkAuthority.Server,
                networkArchetypeId,
                entity =>
                {
                    entity.Set(new StatusTarget { Value = target.GID });
                    entity.Set(new LifeTime { EndTick = simulationTime.DeadlineAfter(spec.Duration) });
                    entity.Set(new StatusStrength
                    {
                        Power = spec.Power,
                        Stacks = NormalizeStacks(spec.Stacks),
                    });
                    entity.Set(new StatusContext
                    {
                        Source = source,
                        RequestId = requestId,
                        RootEffectId = chain.RootEffectId,
                        ChainDepth = chain.Depth,
                        MaxDepth = chain.MaxDepth,
                    });
                    initialize(entity);
                });
        }

        private static byte NormalizeStacks(byte stacks)
        {
            return Math.Max((byte)1, stacks);
        }
    }
}
