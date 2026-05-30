using System;
using System.Collections.Generic;
using System.Threading;
using Aspid.MVVM;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows.Examples
{
    public static class EcsWindowsExample
    {
        public static ExampleSession Create()
        {
            if (ExampleW.Status != WorldStatus.NotCreated)
                ExampleW.Destroy();

            ExampleW.Create(WorldConfig.Default());
            ExampleW.Types()
                .Event<CloseTopEcsWindowRequest>()
                .Event<CloseAllEcsWindowsRequest>()
                .Event<SetAllEcsWindowsPresentationActiveRequest>()
                .Event<OpenInventoryWindowRequest>()
                .Event<CloseInventoryWindowRequest>();
            ExampleW.Initialize();

            var windows = new WindowsController<ExampleWorld>();
            ExampleW.SetResource(windows);
            ExampleW.SetResource(new InventoryWindowState
            {
                ItemCount = 3,
                Gold = 100
            });

            var inventoryShell = new InventoryWindowShellView();
            windows.RegisterWindow<InventoryWindow, InventoryWindowInput, InventoryWindowShellView>(
                () => inventoryShell,
                EcsWindowLayer.Popup);

            windows.RegisterViewModel<InventoryWindow, InventoryWindowInput, InventoryHeaderSlot, InventoryHeaderViewModel>(
                static _ => new InventoryHeaderViewModel(),
                ApplyHeaderOpenInput);

            windows.RegisterViewModel<InventoryWindow, InventoryWindowInput, InventoryItemsSlot, InventoryItemsViewModel>(
                static _ => new InventoryItemsViewModel(),
                ApplyItemsOpenInput);

            windows.RegisterOpenRequest<InventoryWindow, InventoryWindowInput, OpenInventoryWindowRequest>();
            windows.RegisterCloseRequest<InventoryWindow, CloseInventoryWindowRequest>();

            var requestSystem = new EcsWindowRequestSystem<ExampleWorld>();
            requestSystem.Init();

            var itemsBridgeSystem = new InventoryItemsBridgeSystem();
            itemsBridgeSystem.Init();

            return new ExampleSession(windows, inventoryShell, requestSystem, itemsBridgeSystem);
        }

        public static void Run()
        {
            using var session = Create();

            session.OpenInventory(ownerEntityId: 12);
            session.SetInventoryState(itemCount: 5, gold: 125);
            session.UpdateFrame();
            session.CloseInventory();
        }

        private static void ApplyHeaderOpenInput(
            InventoryHeaderViewModel viewModel,
            in EcsWindowContext<ExampleWorld, InventoryWindow, InventoryWindowInput> context)
        {
            viewModel.ApplyOpenInput(context.Input.OwnerEntityId);
        }

        private static void ApplyItemsOpenInput(
            InventoryItemsViewModel viewModel,
            in EcsWindowContext<ExampleWorld, InventoryWindow, InventoryWindowInput> context)
        {
            viewModel.ApplyOpenInput(context.Input.OwnerEntityId);
        }

        public readonly struct InventoryWindow : IEcsWindow
        {
        }

        public readonly struct InventoryHeaderSlot : IEcsWindowSlot
        {
        }

        public readonly struct InventoryItemsSlot : IEcsWindowSlot
        {
        }

        public readonly struct InventoryWindowInput
        {
            public readonly int OwnerEntityId;

            public InventoryWindowInput(int ownerEntityId)
            {
                OwnerEntityId = ownerEntityId;
            }
        }

        public struct InventoryWindowState : IResource
        {
            public int ItemCount;
            public int Gold;
        }

        public readonly struct OpenInventoryWindowRequest
            : IEcsWindowOpenRequest<InventoryWindow, InventoryWindowInput>
        {
            public OpenInventoryWindowRequest(InventoryWindowInput input)
            {
                Input = input;
            }

            public InventoryWindowInput Input { get; }
        }

        public readonly struct CloseInventoryWindowRequest : IEcsWindowCloseRequest<InventoryWindow>
        {
        }

        public sealed class InventoryHeaderViewModel : IViewModel
        {
            public int OwnerEntityId { get; private set; }

            public void ApplyOpenInput(int ownerEntityId)
            {
                OwnerEntityId = ownerEntityId;
            }

            public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
            {
                return default;
            }
        }

        public sealed class InventoryItemsViewModel : IViewModel
        {
            public int OwnerEntityId { get; private set; }
            public int ItemCount { get; private set; }
            public int Gold { get; private set; }

            public void ApplyOpenInput(int ownerEntityId)
            {
                OwnerEntityId = ownerEntityId;
            }

            public void SyncInventoryState(int itemCount, int gold)
            {
                ItemCount = itemCount;
                Gold = gold;
            }

            public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
            {
                return default;
            }
        }

        public sealed class InventoryWindowShellView : IEcsWindowShellView
        {
            private readonly Dictionary<Type, TestAspidView> _slotsByType = new()
            {
                [typeof(InventoryHeaderSlot)] = new TestAspidView(),
                [typeof(InventoryItemsSlot)] = new TestAspidView()
            };

            public bool IsShown { get; private set; }
            public bool IsDisposed { get; private set; }
            public bool IsPresentationActive { get; private set; } = true;
            public bool IsFocused { get; private set; }
            public EcsWindowOrdering Ordering { get; private set; }

            public IViewModel HeaderViewModel => _slotsByType[typeof(InventoryHeaderSlot)].ViewModel;

            public IViewModel ItemsViewModel => _slotsByType[typeof(InventoryItemsSlot)].ViewModel;

            public void SetDrawOrder(EcsWindowOrdering order)
            {
                Ordering = order;
            }

            public UniTask ShowAsync(CancellationToken ct)
            {
                IsShown = true;
                return UniTask.CompletedTask;
            }

            public UniTask HideAsync(CancellationToken ct, bool isInstant = false)
            {
                IsShown = false;
                return UniTask.CompletedTask;
            }

            public void SetPresentationActive(bool isActive)
            {
                IsPresentationActive = isActive;
            }

            public void Focus()
            {
                IsFocused = true;
            }

            public void Blur()
            {
                IsFocused = false;
            }

            public void Bind(Type slotType, IViewModel viewModel)
            {
                _slotsByType[slotType].Initialize(viewModel);
            }

            public void Unbind(Type slotType)
            {
                _slotsByType[slotType].Deinitialize();
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public sealed class InventoryItemsBridgeSystem
            : EcsWindowPresentationBridgeSystem<ExampleWorld, InventoryWindow, InventoryItemsSlot, InventoryItemsViewModel>
        {
            protected override void SyncPresentation(InventoryItemsViewModel viewModel)
            {
                ref readonly var state = ref ExampleW.GetResource<InventoryWindowState>();
                viewModel.SyncInventoryState(state.ItemCount, state.Gold);
            }
        }

        public sealed class ExampleSession : IDisposable
        {
            private readonly WindowsController<ExampleWorld> _windows;
            private readonly EcsWindowRequestSystem<ExampleWorld> _requestSystem;
            private readonly InventoryItemsBridgeSystem _itemsBridgeSystem;
            private bool _isDisposed;

            public ExampleSession(
                WindowsController<ExampleWorld> windows,
                InventoryWindowShellView inventoryShell,
                EcsWindowRequestSystem<ExampleWorld> requestSystem,
                InventoryItemsBridgeSystem itemsBridgeSystem)
            {
                _windows = windows;
                InventoryShell = inventoryShell;
                _requestSystem = requestSystem;
                _itemsBridgeSystem = itemsBridgeSystem;
            }

            public InventoryWindowShellView InventoryShell { get; }

            public InventoryHeaderViewModel HeaderViewModel =>
                _windows.GetViewModel<InventoryWindow, InventoryHeaderSlot, InventoryHeaderViewModel>();

            public InventoryItemsViewModel ItemsViewModel =>
                _windows.GetViewModel<InventoryWindow, InventoryItemsSlot, InventoryItemsViewModel>();

            public void OpenInventory(int ownerEntityId)
            {
                ExampleW.SendEvent(new OpenInventoryWindowRequest(new InventoryWindowInput(ownerEntityId)));
                UpdateFrame();
            }

            public void CloseInventory()
            {
                ExampleW.SendEvent(new CloseInventoryWindowRequest());
                UpdateFrame();
            }

            public void SetInventoryState(int itemCount, int gold)
            {
                ref var state = ref ExampleW.GetResource<InventoryWindowState>();
                state.ItemCount = itemCount;
                state.Gold = gold;
            }

            public void UpdateFrame()
            {
                _requestSystem.Update();
                _itemsBridgeSystem.Update();
                ExampleW.Tick();
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _requestSystem.Destroy();
                _windows.Dispose();

                if (ExampleW.Status != WorldStatus.NotCreated)
                    ExampleW.Destroy();
            }
        }

        public readonly struct ExampleWorld : IWorldType
        {
        }

        public abstract class ExampleW : World<ExampleWorld>
        {
        }

        private sealed class TestAspidView : IView
        {
            public IViewModel ViewModel { get; private set; }

            public void Initialize(IViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public void Deinitialize()
            {
                ViewModel = null;
            }
        }
    }
}
