using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public sealed class StatusEntityFactory : NetEntityFactory<StatusNetworkEntity>, IResource
    {
        private enum StatusSpawnKind
        {
            Poison,
            Burning,
            Oiled
        }

        public EntityGID SpawnPoison(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                StatusSpawnKind.Poison,
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.POISON);
        }

        public EntityGID SpawnBurning(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                StatusSpawnKind.Burning,
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.BURNING);
        }

        public EntityGID SpawnOiled(
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
        {
            return Spawn(
                StatusSpawnKind.Oiled,
                target,
                source,
                spec,
                requestId,
                chain,
                simulationTime,
                StatusNetworkArchetypes.OILED);
        }

        private EntityGID Spawn(
            StatusSpawnKind kind,
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime,
            ushort networkArchetypeId)
        {
            var owner = target.Read<NetworkIdentity>().Owner;

            var entity = CreateEntity(
                owner,
                NetworkAuthority.Server,
                networkArchetypeId);
            Configure(entity, kind, target, source, spec, requestId, chain, simulationTime);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(
            SW.Entity entity,
            StatusSpawnKind kind,
            SW.Entity target,
            EntityGID source,
            AddStatusSpec spec,
            uint requestId,
            EffectChainData chain,
            SimulationTime simulationTime)
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
            ConfigureSubtype(entity, kind, spec, simulationTime);
        }

        private static void ConfigureSubtype(
            SW.Entity entity,
            StatusSpawnKind kind,
            in AddStatusSpec spec,
            SimulationTime simulationTime)
        {
            if (kind == StatusSpawnKind.Oiled)
            {
                entity.Set<OiledStatus>();
                return;
            }

            var intervalTicks = simulationTime.SecondsToTicks(spec.TickInterval);
            entity.Set(new StatusTickState
            {
                IntervalTicks = intervalTicks,
                NextTick = intervalTicks == 0 ? 0 : simulationTime.ServerTick + intervalTicks,
            });

            if (kind == StatusSpawnKind.Poison)
            {
                entity.Set<PoisonStatus>();
                return;
            }

            entity.Set<BurningStatus>();
        }

        private static byte NormalizeStacks(byte stacks)
        {
            return Math.Max((byte)1, stacks);
        }
    }
}
