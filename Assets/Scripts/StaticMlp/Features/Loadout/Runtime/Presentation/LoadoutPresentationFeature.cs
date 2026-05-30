using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
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
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<LoadoutPreparationViewModel, LoadoutPreparationViewData>(
                static (viewModel, in data) => viewModel.Apply(in data));

            windows.RegisterWindow<LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<LoadoutPreparationView>(BUILD_PREPARATION_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Fullscreen);
            windows.RegisterLinkedViewModel<LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationSlot, LoadoutPreparationViewModel>(
                static _ => new LoadoutPreparationViewModel(),
                static _ => ResolveSingletonPresentationEntity<LoadoutPreparationViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, LoadoutPreparationWindow, EcsWindowNoData, LoadoutPreparationViewModel>);

            systems.Add(new ClientLoadoutPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 20);
            systems.Add(new ClientLoadoutPreparationIntentSystem(), GameplaySystemOrder.ClientPresentation + 24);
            systems.Add(new ClientLoadoutPreparationViewDataSystem(), GameplaySystemOrder.ClientPresentation + 25);
        }

        private static EntityGID ResolveSingletonPresentationEntity<TComponent>()
            where TComponent : struct, IComponent
        {
            foreach (var entity in CW.Query<All<TComponent>>().Entities())
                return entity.GID;

            throw new System.InvalidOperationException($"{typeof(TComponent).FullName} entity is missing.");
        }
    }
}
