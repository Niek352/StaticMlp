using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionPresentationFeature : GameplayFeature
    {
        private const string REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH = "Views/Stage1/RewardResultPopupView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var bridge = new RewardResultPopupBridgeSystem();
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<RewardResultPopupViewModel, RewardResultPopupViewData>(
                Binding);

            windows.RegisterWindow<RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<RewardResultPopupShellView>(REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Popup);
            windows.RegisterLinkedViewModel<RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupSlot, RewardResultPopupViewModel>(
                static _ => new RewardResultPopupViewModel(),
                static _ => ResolveSingletonPresentationEntity<RewardResultPopupViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupViewModel>);

            systems.Add(new ClientProgressionPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(new ClientRewardResultPopupStateSystem(), GameplaySystemOrder.ClientPresentation + 41);
            systems.Add(new ClientRewardResultPopupIntentSystem(), GameplaySystemOrder.ClientPresentation + 43);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, RewardResultPopupWindow, RewardResultPopupSession>(
                static (in RewardResultPopupSession session) => session.IsVisible), GameplaySystemOrder.ClientPresentation + 44);
            systems.Add(bridge, GameplaySystemOrder.ClientPresentation + 45);
        }

        private static void Binding(RewardResultPopupViewModel viewModel, in RewardResultPopupViewData data)
        {
            viewModel.Apply(in data);
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
