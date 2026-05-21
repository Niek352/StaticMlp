using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientConstructionViewStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in CW.Query<All<ConstructionTransform, ViewTransform>>().Entities())
            {
                ref readonly var transform = ref e.Read<ConstructionTransform>();
                ref var viewTransform = ref e.Mut<ViewTransform>();
                viewTransform.RenderPosition = transform.Position;
                viewTransform.RenderRotation = transform.Rotation;
            }

            foreach (var e in CW.Query<All<ConstructionSiteState, ConstructionResources, ConstructionProgress, ConstructionViewState>>().Entities())
            {
                ref readonly var progress = ref ClientProjection.Read<ConstructionProgress>(e);
                var next = new ConstructionViewState
                {
                    Phase = ClientProjection.Read<ConstructionSiteState>(e).Phase,
                    WoodRequired = ConstructionResourcesAccess.GetProjectedRequired(e, ResourceCatalog.WoodId),
                    StoneRequired = ConstructionResourcesAccess.GetProjectedRequired(e, ResourceCatalog.StoneId),
                    WoodDelivered = ConstructionResourcesAccess.GetProjectedDelivered(e, ResourceCatalog.WoodId),
                    StoneDelivered = ConstructionResourcesAccess.GetProjectedDelivered(e, ResourceCatalog.StoneId),
                    Progress01 = progress.Normalized
                };

                ref var existing = ref e.Mut<ConstructionViewState>();
                existing = next;
            }
        }
    }
}
