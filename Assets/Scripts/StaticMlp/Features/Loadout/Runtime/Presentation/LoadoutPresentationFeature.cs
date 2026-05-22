using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutPresentationFeature : GameplayFeature
    {
        private const string BUILD_PREPARATION_VIEW_RESOURCE_PATH = "Views/Stage1/LoadoutPreparationView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var bridge = new ControllerResourceBridgeSystem<LoadoutPreparationController, LoadoutPreparationScreenState>();

            systems.Add(new ClientLoadoutPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 20);
            systems.Add(new ClientLoadoutPreparationScreenStateSystem(), GameplaySystemOrder.ClientPresentation + 21);
            systems.Add(new ClientLoadoutHudStateSystem(), GameplaySystemOrder.ClientPresentation + 22);
            systems.Add(new ControllerRegistrationSystem<LoadoutPreparationView, LoadoutPreparationController>(
                new LoadoutPreparationController(ResourcesViewFactory.CreateLazy<LoadoutPreparationView>(BUILD_PREPARATION_VIEW_RESOURCE_PATH), bridge)), GameplaySystemOrder.ClientPresentation + 24);
            systems.Add(bridge, GameplaySystemOrder.ClientPresentation + 25);
        }
    }
}
