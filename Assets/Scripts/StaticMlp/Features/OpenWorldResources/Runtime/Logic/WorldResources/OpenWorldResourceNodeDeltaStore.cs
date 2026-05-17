using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    [Obsolete("Temp")]
    public sealed class OpenWorldResourceNodeDeltaStore : IResource
    {
        private readonly HashSet<long> _depletedPlacementIds = new();

        public int DepletedPlacementCount => _depletedPlacementIds.Count;

        public bool IsDepleted(long placementId)
        {
            return _depletedPlacementIds.Contains(placementId);
        }

        public void RecordDepleted(long placementId)
        {
            _depletedPlacementIds.Add(placementId);
        }
    }
}
