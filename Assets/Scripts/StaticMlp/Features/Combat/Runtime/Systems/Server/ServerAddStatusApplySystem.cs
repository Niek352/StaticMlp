using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Combat
{
    public sealed class ServerAddStatusApplySystem : ISystem
    {
        private readonly List<EntityGID> _effects = new();

        public void Update()
        {
            _effects.Clear();
            foreach (var effect in SW.Query<All<EffectTag, AddStatusEffectTag, EffectSource, EffectTarget, EffectRequestId, AddStatusData, EffectChainData>, None<EffectProcessedTag, EffectRejectedTag>>().Entities())
                _effects.Add(effect.GID);

            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].TryUnpack<ServerWT>(out var effect))
                    Apply(effect);
            }
        }

        private static void Apply(SW.Entity effect)
        {
            ref readonly var targetRef = ref effect.Read<EffectTarget>();
            if (!targetRef.Value.TryUnpack<ServerWT>(out var target))
            {
                effect.Set<EffectProcessedTag>();
                return;
            }

            ref readonly var sourceRef = ref effect.Read<EffectSource>();
            ref readonly var data = ref effect.Read<AddStatusData>();
            ref readonly var requestRef = ref effect.Read<EffectRequestId>();
            ref readonly var chain = ref effect.Read<EffectChainData>();

            switch (data.Type)
            {
                case StatusType.Poison:
                    ApplyPoison(target, sourceRef.Value, data, requestRef.Value, chain);
                    break;
                case StatusType.Burning:
                    ApplyBurning(target, sourceRef.Value, data, requestRef.Value, chain);
                    break;
                case StatusType.Oiled:
                    ApplyOiled(target, sourceRef.Value, data, requestRef.Value, chain);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported status type {data.Type}.");
            }

            effect.Set<EffectProcessedTag>();
        }

        private static void ApplyPoison(SW.Entity target, EntityGID source, AddStatusData data, uint requestId, EffectChainData chain)
        {
            if (target.Has<PoisonStatus>())
            {
                ref var status = ref StaticMlp.Networking.Replication.ReplicationMut.Mut<PoisonStatus>(target);
                status.RemainingTime = Math.Max(status.RemainingTime, data.Duration);
                status.Power = Math.Max(status.Power, data.Power);
                status.Stacks = (byte)Math.Min(byte.MaxValue, status.Stacks + Math.Max((byte)1, data.Stacks));
                status.TickInterval = data.TickInterval;
                status.Source = source;
                status.RequestId = requestId;
                status.RootEffectId = chain.RootEffectId;
                status.ChainDepth = chain.Depth;
                status.MaxDepth = chain.MaxDepth;
                return;
            }

            target.Set(new PoisonStatus
            {
                RemainingTime = data.Duration,
                TickInterval = data.TickInterval,
                TickTimer = 0f,
                Power = data.Power,
                Stacks = Math.Max((byte)1, data.Stacks),
                Source = source,
                RequestId = requestId,
                RootEffectId = chain.RootEffectId,
                ChainDepth = chain.Depth,
                MaxDepth = chain.MaxDepth,
            });
        }

        private static void ApplyBurning(SW.Entity target, EntityGID source, AddStatusData data, uint requestId, EffectChainData chain)
        {
            if (target.Has<BurningStatus>())
            {
                ref var status = ref StaticMlp.Networking.Replication.ReplicationMut.Mut<BurningStatus>(target);
                status.RemainingTime = Math.Max(status.RemainingTime, data.Duration);
                status.Power = Math.Max(status.Power, data.Power);
                status.Stacks = (byte)Math.Min(byte.MaxValue, status.Stacks + Math.Max((byte)1, data.Stacks));
                status.TickInterval = data.TickInterval;
                status.Source = source;
                status.RequestId = requestId;
                status.RootEffectId = chain.RootEffectId;
                status.ChainDepth = chain.Depth;
                status.MaxDepth = chain.MaxDepth;
                return;
            }

            target.Set(new BurningStatus
            {
                RemainingTime = data.Duration,
                TickInterval = data.TickInterval,
                TickTimer = 0f,
                Power = data.Power,
                Stacks = Math.Max((byte)1, data.Stacks),
                Source = source,
                RequestId = requestId,
                RootEffectId = chain.RootEffectId,
                ChainDepth = chain.Depth,
                MaxDepth = chain.MaxDepth,
            });
        }

        private static void ApplyOiled(SW.Entity target, EntityGID source, AddStatusData data, uint requestId, EffectChainData chain)
        {
            if (target.Has<OiledStatus>())
            {
                ref var status = ref StaticMlp.Networking.Replication.ReplicationMut.Mut<OiledStatus>(target);
                status.RemainingTime = Math.Max(status.RemainingTime, data.Duration);
                status.Power = Math.Max(status.Power, data.Power);
                status.Stacks = (byte)Math.Min(byte.MaxValue, status.Stacks + Math.Max((byte)1, data.Stacks));
                status.Source = source;
                status.RequestId = requestId;
                status.RootEffectId = chain.RootEffectId;
                status.ChainDepth = chain.Depth;
                status.MaxDepth = chain.MaxDepth;
                return;
            }

            target.Set(new OiledStatus
            {
                RemainingTime = data.Duration,
                Power = data.Power,
                Stacks = Math.Max((byte)1, data.Stacks),
                Source = source,
                RequestId = requestId,
                RootEffectId = chain.RootEffectId,
                ChainDepth = chain.Depth,
                MaxDepth = chain.MaxDepth,
            });
        }
    }
}
