using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerAiRuntimeInitSystem : ISystem
    {
        public void Update()
        {
            if (SW.HasResource<AiBehaviorCatalog>())
            {
                var existingCatalog = SW.GetResource<AiBehaviorCatalog>();
                if (existingCatalog == null)
                    throw new InvalidOperationException("AI behavior catalog resource exists but is null.");

                return;
            }

            SW.SetResource(AiBehaviorCatalogDefaults.Create());
        }
    }
}
