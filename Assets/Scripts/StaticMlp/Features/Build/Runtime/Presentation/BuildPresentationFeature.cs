using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Build
{
    public sealed class BuildPresentationFeature : GameplayFeature
    {
        private const string BUILD_PREPARATION_VIEW_RESOURCE_PATH = "Views/Stage1/BuildPreparationView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var bridge = new ControllerResourceBridgeSystem<BuildPreparationController, BuildPreparationScreenState>();

            systems.Add(new ClientBuildPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 20);
            systems.Add(new ClientBuildPreparationScreenStateSystem(), GameplaySystemOrder.ClientPresentation + 21);
            systems.Add(new ControllerRegistrationSystem<BuildPreparationView, BuildPreparationController>(
                new BuildPreparationController(ResourcesViewFactory.CreateLazy<BuildPreparationView>(BUILD_PREPARATION_VIEW_RESOURCE_PATH), bridge)), GameplaySystemOrder.ClientPresentation + 23);
            systems.Add(bridge, GameplaySystemOrder.ClientPresentation + 24);
        }
    }
}
