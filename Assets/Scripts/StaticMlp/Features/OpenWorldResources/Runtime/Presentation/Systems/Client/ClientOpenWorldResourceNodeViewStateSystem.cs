using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceNodeViewStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState, OpenWorldResourceNodeTransform, ViewTransform, OpenWorldResourceNodeViewState>>().Entities())
            {
                ref readonly var nodeTransform = ref ClientProjection.Read<OpenWorldResourceNodeTransform>(entity);
                ref var viewTransform = ref entity.Mut<ViewTransform>();
                viewTransform.RenderPosition = nodeTransform.Position;
                viewTransform.RenderRotation = Quaternion.Euler(0f, nodeTransform.YawDegrees, 0f);

                ref readonly var nodeState = ref ClientProjection.Read<OpenWorldResourceNodeState>(entity);
                ref var viewState = ref entity.Mut<OpenWorldResourceNodeViewState>();
                viewState.KindIdValue = nodeState.KindIdValue;
                viewState.RemainingAmount = nodeState.RemainingAmount;
                viewState.Scale = nodeTransform.Scale;
            }
        }
    }
}
