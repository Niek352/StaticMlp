using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerApplyConstructionBuildWorkSystem : ISystem
    {
        private EventReceiver<ServerWT, ApplyConstructionBuildWorkEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<ApplyConstructionBuildWorkEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var request in _requests)
                Handle(in request.Value);
        }

        private static void Handle(in ApplyConstructionBuildWorkEvent request)
        {
            if (!request.Site.TryUnpack<ServerWT>(out var site))
                throw new InvalidOperationException($"Construction build work target {request.Site} is not a server entity.");

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var progress = ref ReplicationMut.Mut<ConstructionProgress>(site);
            SettlementConstructionRules.ApplyBuildWork(
                site,
                ref state,
                ref progress,
                request.WorkAmount,
                request.MaxWork);
        }
    }
}
