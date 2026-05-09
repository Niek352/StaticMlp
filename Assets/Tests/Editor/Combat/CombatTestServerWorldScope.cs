using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CombatTestServerWorldScope : IDisposable
    {
        public CombatTestServerWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            NetworkEventRegistry.Clear();
            new CombatFeature().RegisterNetworkEvents();
            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(CombatFeature).Assembly,
                typeof(CharacterNetState).Assembly,
                typeof(StaticMlp.Features.AiBots.AiAgentTag).Assembly);
            NetworkEventRegistry.RegisterServerWorldTypes();
            SW.Initialize();
            SW.SetResource(new CombatDebugLogBuffer());
            SW.SetResource(new CombatAutoAttackConfig());
        }

        public CombatDebugLogBuffer DebugLog => SW.GetResource<CombatDebugLogBuffer>();

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

        public void Dispose()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
