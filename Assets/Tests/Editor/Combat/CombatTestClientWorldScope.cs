using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CombatTestClientWorldScope : IDisposable
    {
        public CombatTestClientWorldScope()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            NetworkEventRegistry.Clear();
            new BuildLogicFeature().RegisterNetworkEvents();
            new CombatLogicFeature().RegisterNetworkEvents();
            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(BuildLogicFeature).Assembly,
                typeof(CombatLogicFeature).Assembly,
                typeof(CombatPresentationFeature).Assembly,
                typeof(EffectsLogicFeature).Assembly,
                typeof(EffectsPresentationFeature).Assembly,
                typeof(Health).Assembly,
                typeof(StatusesLogicFeature).Assembly,
                typeof(StatusesPresentationFeature).Assembly,
                typeof(CharacterNetState).Assembly);
            NetworkEventRegistry.RegisterClientWorldTypes();
            CW.Initialize();
            CW.SetResource(new GameTime());
            CW.SetResource(new CombatConfig());
            CW.SetResource(new StatusesConfig());
            CW.SetResource(new CombatPresentationConfig());
        }

        public CombatConfig Config => CW.GetResource<CombatConfig>();
        public CombatPresentationConfig PresentationConfig => CW.GetResource<CombatPresentationConfig>();
        public GameTime Time => CW.GetResource<GameTime>();

        public void SetGameTime(float time, float deltaTime = 0f)
        {
            var gameTime = Time;
            gameTime.Time = time;
            gameTime.DeltaTime = deltaTime;
        }

        public CW.Entity CreateLocalPlayer(Vector3 position, float currentHealth = 100f, float maxHealth = 100f)
        {
            var player = CW.NewEntity<Default>();
            player.Set<LocalOwned>();
            player.Set<PlayerTag>();
            player.Set(new Health
            {
                Current = currentHealth,
                Max = maxHealth
            });
            player.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            player.Set(Stage1BuildRules.DefaultSelection());
            player.Set(Stage1BuildRules.CreatePreparedSnapshot(player.Read<OwnerBuildSelection>()));
            return player;
        }

        public CW.Entity CreateMonster(Vector3 position, float? health = 100f)
        {
            var monster = CW.NewEntity<Default>();
            monster.Set<MonsterTag>();
            monster.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });

            if (health.HasValue)
            {
                monster.Set(new Health
                {
                    Current = health.Value,
                    Max = Mathf.Max(0f, health.Value)
                });
            }

            return monster;
        }

        public void Dispose()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();
        }
    }
}
