using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ServerResourcePickupSpawnSystem : ISystem
    {
        private const float HIT_POINT_QUANTIZATION = 0.01f;

        private EventReceiver<ServerWT, OpenWorldResourceHarvestedEvent> _harvested;

        public void Init()
        {
            _harvested = SW.RegisterEventReceiver<OpenWorldResourceHarvestedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _harvested);
        }

        public void Update()
        {
            var factory = SW.GetResource<ResourcePickupFactory>();

            foreach (var evt in _harvested)
            {
                var harvested = evt.Value;
                ValidateHarvestedResource(harvested.Resource);
                factory.Spawn(
                    harvested.Resource,
                    ToWorldPosition(in harvested),
                    harvested.SourcePlayer,
                    harvested.PlacementId);
            }
        }

        private static Vector3 ToWorldPosition(in OpenWorldResourceHarvestedEvent harvested)
        {
            var position = new Vector3(
                harvested.HitPointXQ * HIT_POINT_QUANTIZATION,
                harvested.HitPointYQ * HIT_POINT_QUANTIZATION,
                harvested.HitPointZQ * HIT_POINT_QUANTIZATION);
            if (!IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z))
                throw new InvalidOperationException($"Harvested resource pickup position is non-finite for placement {harvested.PlacementId}.");

            return position;
        }

        private static void ValidateHarvestedResource(ResourceAmount resource)
        {
            if (resource.Amount <= 0)
                throw new InvalidOperationException($"Harvested resource id {resource.Id.Value} has invalid amount {resource.Amount}.");

            ref readonly var definition = ref ResourceCatalog.Get(resource.Id);
            if (definition.Family != ResourceFamily.Raw)
                throw new InvalidOperationException($"Harvested resource id {resource.Id.Value} is {definition.Family}, not raw.");
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
