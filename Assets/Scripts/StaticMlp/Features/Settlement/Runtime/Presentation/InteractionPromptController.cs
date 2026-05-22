using Code.EcsUi.Mvc;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptController : ControllerBase<InteractionPromptView>
    {
        public InteractionPromptController(
            ViewFactoryMethod<InteractionPromptView> viewFactory,
            InteractionPromptBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<InteractionPromptBridgeSystem, InteractionPromptController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 95;

        public void Apply(in InteractionPromptState state)
        {
            View.Render(in state);
        }
    }
}
