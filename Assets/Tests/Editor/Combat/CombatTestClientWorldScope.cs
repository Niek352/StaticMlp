using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CombatTestClientWorldScope : IDisposable
    {
        public CombatTestClientWorldScope()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(CombatFeature).Assembly,
                typeof(CharacterNetState).Assembly);
            CW.Initialize();
            CW.SetResource(new CombatAutoAttackConfig());
        }

        public CombatAutoAttackConfig Config => CW.GetResource<CombatAutoAttackConfig>();

        public CW.Entity CreateLocalPlayer(Vector3 position)
        {
            var player = CW.NewEntity<Default>();
            player.Set<LocalOwned>();
            player.Set<PlayerTag>();
            player.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
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
