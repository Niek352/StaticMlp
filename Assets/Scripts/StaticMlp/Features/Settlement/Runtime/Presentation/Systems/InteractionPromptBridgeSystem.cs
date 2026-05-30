using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptBridgeSystem : ISystem
    {
        public void Update()
        {
            ref readonly var state = ref CW.GetResource<InteractionPromptState>();
            foreach (var entity in CW.Query<All<InteractionPromptViewData>>().Entities())
            {
                ref var data = ref entity.Mut<InteractionPromptViewData>();
                data.State = state;
                return;
            }

            throw new InvalidOperationException($"{nameof(InteractionPromptViewData)} entity is missing.");
        }
    }
}
