using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.EcsViews
{
    public sealed class DestroyEntityViewSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<View, DestroyViewRequest>>().Entities())
            {
                entity.Delete<View>();
                entity.Delete<DestroyViewRequest>();
            }
        }
    }
}
