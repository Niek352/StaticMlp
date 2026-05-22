using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Loadout
{
    public sealed class ClientLoadoutHudStateSystem : ISystem
    {
        public void Update()
        {
            var next = new LoadoutHudState();

            foreach (var player in CW.Query<All<PreparedLoadoutSnapshot>>().Entities())
            {
                var snapshot = player.Read<PreparedLoadoutSnapshot>();
                next.PreparedPrimaryModuleId = snapshot.PrimaryModuleId;
                next.HasPreparedBuild = snapshot.PrimaryModuleId.Value != 0;
                break;
            }

            CW.SetResource(next);
        }
    }
}
