using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public static class LoadoutHudPresentation
    {
        public static LoadoutHudState Build()
        {
            var state = new LoadoutHudState();

            foreach (var player in CW.Query<All<PreparedLoadoutSnapshot>>().Entities())
            {
                var snapshot = player.Read<PreparedLoadoutSnapshot>();
                state.PreparedPrimaryModuleId = snapshot.PrimaryModuleId;
                state.HasPreparedBuild = snapshot.PrimaryModuleId.Value != 0;
                break;
            }

            return state;
        }
    }
}
