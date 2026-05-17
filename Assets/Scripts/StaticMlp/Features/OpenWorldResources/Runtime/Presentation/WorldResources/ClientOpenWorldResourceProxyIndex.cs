using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceProxyIndex : IResource
    {
        private readonly Dictionary<long, EntityGID> _proxyByPlacementId = new();

        public void Register(long placementId, EntityGID entity)
        {
            if (_proxyByPlacementId.TryGetValue(placementId, out var existing) && existing != entity)
                throw new InvalidOperationException($"Resource proxy for placement {placementId} is already registered.");

            _proxyByPlacementId[placementId] = entity;
        }

        public void Unregister(long placementId)
        {
            _proxyByPlacementId.Remove(placementId);
        }

        public bool TryGet(long placementId, out EntityGID entity)
        {
            return _proxyByPlacementId.TryGetValue(placementId, out entity);
        }
    }
}
