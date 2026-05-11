using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Statuses;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CombatTestServerWorldScope : IDisposable
    {
        public const float DEFAULT_FIXED_STEP_SECONDS = 1f / 30f;

        public CombatTestServerWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            ServerPeerRegistry.Clear();
            NetworkEventRegistry.Clear();
            new BuildLogicFeature().RegisterNetworkEvents();
            new CombatLogicFeature().RegisterNetworkEvents();
            new FrontierLogicFeature().RegisterNetworkEvents();
            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(BuildLogicFeature).Assembly,
                typeof(CombatLogicFeature).Assembly,
                typeof(EffectsLogicFeature).Assembly,
                typeof(FrontierLogicFeature).Assembly,
                typeof(Health).Assembly,
                typeof(ProgressionLogicFeature).Assembly,
                typeof(SettlementSharedResourcesGameplayFeature).Assembly,
                typeof(StatusesLogicFeature).Assembly,
                typeof(CharacterNetState).Assembly,
                typeof(Features.AiBots.AiAgentTag).Assembly);
            NetworkEventRegistry.RegisterServerWorldTypes();
            SW.Initialize();
            SW.SetResource(new GameTime());
            SW.SetResource(new SimulationTime
            {
                FixedStepSeconds = DEFAULT_FIXED_STEP_SECONDS,
            });
            SW.SetResource(new CombatDebugLogBuffer());
            SW.SetResource(new CombatConfig());
            SW.SetResource(Stage1FrontierSeedManifest.CreateResource());
            SW.SetResource(Stage1ProgressionSeedManifest.CreateResource());
            SW.SetResource(new StatusesConfig());
        }

        public CombatDebugLogBuffer DebugLog => SW.GetResource<CombatDebugLogBuffer>();
        public GameTime Time => SW.GetResource<GameTime>();
        public SimulationTime SimulationTime => SW.GetResource<SimulationTime>();

        public void SetGameTime(float time, float deltaTime = 0f)
        {
            var gameTime = Time;
            gameTime.Time = time;
            gameTime.DeltaTime = deltaTime;
        }

        public void SetSimulationTime(uint serverTick, float fixedStepSeconds = DEFAULT_FIXED_STEP_SECONDS, double? elapsedSeconds = null)
        {
            var simulationTime = SimulationTime;
            simulationTime.ServerTick = serverTick;
            simulationTime.FixedStepSeconds = fixedStepSeconds;
            simulationTime.ElapsedSeconds = elapsedSeconds ?? serverTick * fixedStepSeconds;
        }

        public void AdvanceSimulationTicks(uint ticks)
        {
            var simulationTime = SimulationTime;
            simulationTime.ServerTick += ticks;
            simulationTime.ElapsedSeconds += ticks * simulationTime.FixedStepSeconds;
        }

        public void AdvanceSimulationSeconds(float seconds)
        {
            AdvanceSimulationTicks(SecondsToTicks(seconds));
        }

        public uint SecondsToTicks(float seconds)
        {
            return SimulationTime.SecondsToTicks(seconds);
        }

        public SW.Entity CreateEntity()
        {
            return SW.NewEntity<Default>();
        }

        public SW.Entity CreateEntityWithHealth(float current = 100f, float max = 100f)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new Health
            {
                Current = current,
                Max = max,
            });
            return entity;
        }

        public SW.Entity CreatePlayer(NetworkPeerId owner, Vector3 position)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<PlayerTag>();
            entity.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = NetworkAuthority.Owner,
                NetworkArchetypeId = 0
            });
            entity.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new ServerCombatAttackState());
            entity.Set(new Health
            {
                Current = 100f,
                Max = 100f
            });
            entity.Set(Stage1BuildRules.DefaultSelection());
            entity.Set(Stage1BuildRules.CreatePreparedSnapshot(entity.Read<OwnerBuildSelection>()));
            return entity;
        }

        public SW.Entity CreateMonsterWithHealth(Vector3 position, float current = 100f, float max = 100f)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<MonsterTag>();
            entity.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new ServerCombatAttackState());
            entity.Set(new Health
            {
                Current = current,
                Max = max
            });
            return entity;
        }

        public SW.Entity CreateSettlementAnchor(
            SettlementAnchorId anchorId,
            Vector3 position,
            Stage1SettlementProgressStage stage = Stage1SettlementProgressStage.BuildPrepared)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new Stage1SettlementProgression
            {
                AnchorId = anchorId.Value,
                Stage = stage
            });
            entity.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            return entity;
        }

        public SW.Entity CreateSettlementSharedResources(int wood = 50, int stone = 25)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources
            {
                Wood = wood,
                Stone = stone
            });
            return entity;
        }

        public void Dispose()
        {
            ServerPeerRegistry.Clear();
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
