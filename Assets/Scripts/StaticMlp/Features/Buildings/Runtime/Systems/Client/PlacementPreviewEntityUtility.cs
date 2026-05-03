using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StaticMlp.Features.Buildings
{
    public static class PlacementPreviewEntityUtility
    {
        public static CW.Entity GetOrCreate(in BuildingDefinition definition)
        {
            if (TryGet(out var existing))
            {
                if (existing.Has<PlacementPreview>()
                    && existing.Read<PlacementPreview>().BuildingId == definition.Id.Value)
                    return existing;

                DestroyAll();
            }

            if (!BuildingPresentationCatalog.TryGetDefinition(definition.Id, out var presentation))
                throw new InvalidOperationException($"Missing presentation catalog entry for building {definition.Id}.");

            var created = ClientOnlyEntities.New(ClientBuildingClusters.ClientOnly, ClientBuildingClusters.ClientOnlyChunk);
            created.Set<PlacementPreviewTag>();
            created.Set(new PlacementPreview
            {
                BuildingId = definition.Id.Value,
                Rotation = Quaternion.identity,
                InvalidReason = PlacementInvalidReason.OffGround
            });
            created.Set(new PlacementPreviewViewState
            {
                InvalidReason = PlacementInvalidReason.OffGround
            });
            created.Set(new ViewTransform
            {
                RenderRotation = Quaternion.identity
            });
            created.Set(new ViewPath(presentation.GhostPreviewViewPath));
            return created;
        }

        public static bool TryGet(out CW.Entity entity)
        {
            foreach (var e in CW.Query<All<PlacementPreviewTag>>().Entities())
            {
                entity = e;
                return true;
            }

            entity = default;
            return false;
        }

        public static void DestroyAll()
        {
            var gids = new List<EntityGID>();
            foreach (var e in CW.Query<All<PlacementPreviewTag>>().Entities())
                gids.Add(e.GID);

            for (var i = 0; i < gids.Count; i++)
            {
                if (!gids[i].TryUnpack<ClientCoreWT>(out var e))
                    continue;

                if (e.Has<View>())
                {
                    ref readonly var view = ref e.Read<View>();
                    view.Value.Unbind();

                    if (view.Value is MonoBehaviour monoBehaviour)
                        Object.Destroy(monoBehaviour.gameObject);
                }

                e.Destroy();
            }
        }
    }
}
