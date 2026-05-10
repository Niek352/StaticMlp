using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Statuses
{
    public static class StatusEntityLookup
    {
        public static bool TryFind<TStatusTag>(EntityGID target, out SW.Entity statusEntity)
            where TStatusTag : struct, ITag
        {
            foreach (var entity in SW.Query<All<TStatusTag, StatusTarget>, None<IsDestroyed>>().Entities())
            {
                if (entity.Read<StatusTarget>().Value != target)
                    continue;

                statusEntity = entity;
                return true;
            }

            statusEntity = default;
            return false;
        }
    }
}
