using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelCompositeBridgeSystem : ISystem
    {
        public void Update()
        {
            var session = CW.GetResource<SettlementContextPanelSession>();
            var viewData = new SettlementContextPanelViewData
            {
                Building = BuildingContextPanelPresentation.Build(in session),
                Worker = WorkerContextPanelPresentation.Build(in session),
                Mode = session.Mode
            };

            foreach (var entity in CW.Query<All<SettlementContextPanelViewData>>().Entities())
            {
                ref var data = ref entity.Mut<SettlementContextPanelViewData>();
                data = viewData;
                return;
            }

            throw new InvalidOperationException($"{nameof(SettlementContextPanelViewData)} entity is missing.");
        }
    }
}
