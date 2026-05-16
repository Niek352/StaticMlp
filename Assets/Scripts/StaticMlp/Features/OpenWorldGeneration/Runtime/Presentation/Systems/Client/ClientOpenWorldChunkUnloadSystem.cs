using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class ClientOpenWorldChunkUnloadSystem : ISystem
    {
        private readonly List<EntityGID> _viewEntities = new();
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<OpenWorldChunkUnloadEvent>> _unloads;

        public void Init()
        {
            _unloads = CW.RegisterEventReceiver<NetworkEventFromServer<OpenWorldChunkUnloadEvent>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _unloads);
        }

        public void Update()
        {
            foreach (var unload in _unloads)
                UnloadCluster(unload.Value.Value.ClusterId);
        }

        private void UnloadCluster(ushort clusterId)
        {
            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { clusterId };
            _viewEntities.Clear();
            foreach (var entity in CW.Query<All<View>>().Entities(clusters: clusters))
                _viewEntities.Add(entity.GID);

            for (var i = 0; i < _viewEntities.Count; i++)
            {
                if (!_viewEntities[i].TryUnpack<ClientCoreWT>(out var entity))
                    continue;

                ref readonly var view = ref entity.Read<View>();
                view.Value.Unbind();

                if (view.Value is MonoBehaviour monoBehaviour)
                    UnityEngine.Object.Destroy(monoBehaviour.gameObject);
            }

            CW.Query().BatchUnload(EntityStatusType.Any, clusters: clusters);
            _viewEntities.Clear();
        }
    }
}
