using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ServerOpenWorldResourceNodeDeltaCaptureSystem : ISystem
    {
        public void Update()
        {
            var deltaStore = SW.GetResource<OpenWorldResourceNodeDeltaStore>();
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState>>().Entities())
            {
                ref readonly var state = ref entity.Read<OpenWorldResourceNodeState>();
                if (state.RemainingAmount <= 0)
                    deltaStore.RecordDepleted(state.PlacementId);
            }
        }
    }
}
