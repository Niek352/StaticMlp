using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourcesPresentationFeature : GameplayFeature
    {
        private const string RESOURCE_NODE_VIEW_PATH = "Views/OpenWorldResources/OpenWorldResourceNodeView";

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(OpenWorldResourceNetworkArchetypeIds.ResourceNode, e =>
            {
                e.Set(new ViewPath(RESOURCE_NODE_VIEW_PATH));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
                e.Set(new OpenWorldResourceNodeViewState());
            });
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientOpenWorldResourceNodeViewStateSystem(), ViewSystemOrder.BuildPresentationState);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<OpenWorldResourceNodeViewState>();
        }
    }
}
