using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldResourcesPresentationFeature : GameplayFeature
    {
        private const string RESOURCE_NODE_VIEW_PATH = "Views/OpenWorldResources/OpenWorldResourceNodeView";

        public override void RegisterPrefabs()
        {
            if (!OpenWorldResourcesCompatibility.UseLegacyReplicatedResourceNodes)
                return;

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
            CW.SetResource(new OpenWorldPlacementIndexStore());
            CW.SetResource(new OpenWorldChunkOverlayStore());
            CW.SetResource(new ClientOpenWorldResourceProxyIndex());

            systems.Add(new ClientOpenWorldResourceProxyUnloadSystem(), GameplaySystemOrder.ClientApplyNetworkState + 46);
            systems.Add(new ClientOpenWorldChunkOverlayApplySystem(), GameplaySystemOrder.ClientApplyNetworkState + 50);
            systems.Add(new ClientOpenWorldResourceProxySpawnSystem(), GameplaySystemOrder.ClientPresentation - 30);
            systems.Add(new ClientOpenWorldResourceNodeViewStateSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientOpenWorldResourceNodeViewBindSystem(), ViewSystemOrder.BindViews - 1);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<OpenWorldResourceNodeViewState>();
        }
    }
}
