using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceNodeViewBindSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<OpenWorldResourceProxyTag, ViewTransform, OpenWorldResourceNodeViewState>, None<View>>().Entities())
            {
                var view = OpenWorldResourceNodeRuntimeView.Create(ViewRootProvider.Root);
                view.Bind(entity);
                entity.Set(new View(view));
            }
        }
    }
}
