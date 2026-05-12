using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public sealed class PlayerFactory : NetEntityFactory<PlayerNetworkEntity>, IResource
    {
        private const float DEFAULT_MAX_HEALTH = 100f;

        public EntityGID Spawn(PlayerFactoryData data)
        {
            var entity = CreateEntity(
                data.Owner,
                NetworkAuthority.Owner,
                PlayerGameplayFeature.PLAYER);
            Configure(entity, data);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in PlayerFactoryData data)
        {
            entity.Set<PlayerTag>();
            entity.Set(new ServerCombatAttackState());
            entity.Set(new Health
            {
                Current = DEFAULT_MAX_HEALTH,
                Max = DEFAULT_MAX_HEALTH
            });
            entity.Set(new CharacterNetState
            {
                Position = data.Position,
                Velocity = Vector3.zero,
                Rotation = data.Rotation
            });
        }
    }
}
