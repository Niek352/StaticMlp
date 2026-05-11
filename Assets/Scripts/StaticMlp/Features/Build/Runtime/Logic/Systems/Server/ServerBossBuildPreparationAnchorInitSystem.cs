using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class ServerBossBuildPreparationAnchorInitSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                if (!anchor.Has<BossBuildPreparationState>())
                {
                    anchor.Set(new BossBuildPreparationState
                    {
                        Status = BossBuildPreparationStatus.None
                    });
                }

                if (!anchor.Has<BossPreparedBuildSnapshot>())
                    anchor.Set(new BossPreparedBuildSnapshot());
            }
        }
    }
}
