using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Interaction;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientConstructionInteractableFocusPointSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<InteractableTag, ConstructionTransform>>().Entities())
            {
                ref var focusPoint = ref entity.Mut<InteractableFocusPoint>();
                focusPoint.Position = entity.Read<ConstructionTransform>().Position;
            }
        }
    }
}
