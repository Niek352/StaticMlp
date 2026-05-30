using Aspid.StaticEcs.Windows;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutPresentationFeature : GameplayFeature
    {
        private const string BUILD_PREPARATION_VIEW_RESOURCE_PATH = "Views/Stage1/LoadoutPreparationView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();

            windows.RegisterWindow<LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<LoadoutPreparationView>(BUILD_PREPARATION_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Fullscreen);
            windows.RegisterViewModel<LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationSlot, LoadoutPreparationViewModel>(
                static _ => new LoadoutPreparationViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationViewModel>);

            systems.Add(new ClientLoadoutPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 20);
            systems.Add(new ClientLoadoutPreparationIntentSystem(), GameplaySystemOrder.ClientPresentation + 24);
            systems.Add(new ClientLoadoutPreparationViewModelSyncSystem(), GameplaySystemOrder.ClientPresentation + 25);
        }
    }
}
