using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ServerStage1ProgressionAnchorInitSystem : ISystem
    {
        public void Update()
        {
            var seed = SW.GetResource<Stage1ProgressionSeed>();
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                if (anchor.Has<Stage1ProgressionState>())
                    continue;

                anchor.Set(new Stage1ProgressionState(
                    anchor.Read<Stage1SettlementProgression>().Anchor,
                    BuildFlagMask(seed)));
            }
        }

        private static uint BuildFlagMask(Stage1ProgressionSeed seed)
        {
            var mask = 0u;
            for (var i = 0; i < seed.StartingFlags.Length; i++)
                mask |= Stage1ProgressionState.GetFlagBit(seed.StartingFlags[i]);

            return mask;
        }
    }
}
