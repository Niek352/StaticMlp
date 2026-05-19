using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class DirectorTelegraphReceiveSystem : ISystem
    {
        private const float DEFAULT_SOURCE_RADIUS = 3f;

        public void Update()
        {
            foreach (var enemy in CW.Query<All<EnemySpawnSource>, AllAdded<EnemySpawnSource>>().Entities())
            {
                ref readonly var source = ref enemy.Read<EnemySpawnSource>();
                var telegraph = ClientOnlyEntities.New();
                telegraph.Set(new SpawnSourceTelegraphState
                {
                    SourceType = source.SourceType,
                    Position = ToVector3(source.SpawnPosition),
                    Radius = DEFAULT_SOURCE_RADIUS
                });
            }
        }

        private static Vector3 ToVector3(Unity.Mathematics.float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
    }
}
