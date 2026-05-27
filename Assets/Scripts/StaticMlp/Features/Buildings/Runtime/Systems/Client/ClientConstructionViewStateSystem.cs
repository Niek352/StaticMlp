using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using Unity.Collections;

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
                    Progress01 = progress.Normalized
                };
                CopyProjectedConstructionResources(e, ref next.Resources);

                ref var existing = ref e.Mut<ConstructionViewState>();
                existing = next;
            }
        }

        private static void CopyProjectedConstructionResources(
            CW.Entity entity,
            ref FixedList512Bytes<ConstructionResourceViewEntry> target)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(entity);
            for (var i = 0; i < rows.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(ConstructionViewState)} cannot hold more than {target.Capacity} resource rows.");

                ref var row = ref rows[i].Value;
                target.Add(new ConstructionResourceViewEntry(row.Id, row.Required, row.Delivered));
            }
        }
    }
}
