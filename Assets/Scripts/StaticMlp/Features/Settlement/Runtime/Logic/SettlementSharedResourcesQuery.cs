using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public static class SettlementSharedResourcesQuery
    {
        public static SW.Entity GetServerEntity()
        {
            var found = false;
            var result = default(SW.Entity);

            foreach (var entity in SW.Query<All<SettlementResourceStorageTag, SettlementSharedResources>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Expected a single settlement shared resource storage entity in the server world.");

                found = true;
                result = entity;
            }

            if (!found)
                throw new InvalidOperationException("Settlement shared resource storage entity is missing in the server world.");

            return result;
        }

        public static CW.Entity GetClientEntity()
        {
            var found = false;
            var result = default(CW.Entity);

            foreach (var entity in CW.Query<All<SettlementResourceStorageTag, SettlementSharedResources>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Expected a single settlement shared resource storage entity in the client world.");

                found = true;
                result = entity;
            }

            if (!found)
                throw new InvalidOperationException("Settlement shared resource storage entity is missing in the client world.");

            return result;
        }
    }
}
