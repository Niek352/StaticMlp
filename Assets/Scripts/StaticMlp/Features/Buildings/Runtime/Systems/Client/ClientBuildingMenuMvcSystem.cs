using Code.EcsUi.Mvc;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuMvcSystem : ISystem
    {
        private const string VIEW_RESOURCE_PATH = "Views/Buildings/BuildingMenu";

        private IMvcManager _mvcManager;
        private BuildingMenuController _controller;

        public void Init()
        {
            var prefab = Resources.Load<GameObject>(VIEW_RESOURCE_PATH);
            var viewPrefab = prefab.GetComponent<BuildingMenuView>();
            _mvcManager = CW.GetResource<MvcManagerResource>().Manager;
            _controller = new BuildingMenuController(() => Object.Instantiate(viewPrefab));
            _mvcManager.RegisterController(_controller);
        }

        public void Update()
        {
            ref readonly var state = ref CW.GetResource<BuildingMenuState>();
            if (state.IsOpen)
            {
                if (_controller.State == ControllerState.ViewHidden)
                {
                    _mvcManager.ShowAsync(BuildingMenuController.IssueCommand()).Forget();
                    return;
                }

                if (_controller.State != ControllerState.ViewHiding)
                    _controller.SyncPresentation();
                return;
            }

            if (_controller.State != ControllerState.ViewHidden)
                _mvcManager.TryClose(_controller);
        }
    }
}
