using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudBridgeSystem : ISystem
    {
        public void Update()
        {
            var presentation = ResourcesInventoryHudPresentationBuilder.Build();
            foreach (var entity in CW.Query<All<ResourcesInventoryHudViewData>>().Entities())
            {
                ref var data = ref entity.Mut<ResourcesInventoryHudViewData>();
                data.Presentation = presentation;
                return;
            }

            throw new InvalidOperationException($"{nameof(ResourcesInventoryHudViewData)} entity is missing.");
        }
    }
}
