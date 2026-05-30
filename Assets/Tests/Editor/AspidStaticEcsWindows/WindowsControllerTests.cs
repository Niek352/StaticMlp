using System;
using System.Collections.Generic;
using System.Threading;
using Aspid.MVVM;
using Aspid.StaticEcs.Windows;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace Aspid.StaticEcs.Windows.Tests
{
    public sealed class WindowsControllerTests
    {
        private WindowsController<TestWorld> _controller;
        private EcsWindowRequestSystem<TestWorld> _requestSystem;
        private bool _requestSystemInitialized;

        [SetUp]
        public void SetUp()
        {
            if (TestW.Status != WorldStatus.NotCreated)
                TestW.Destroy();

            TestW.Create(WorldConfig.Default());
            TestW.Types()
                .Event<CloseTopEcsWindowRequest>()
                .Event<CloseAllEcsWindowsRequest>()
                .Event<SetAllEcsWindowsPresentationActiveRequest>()
                .Event<OpenMainWindowRequest>()
                .Event<OpenSecondWindowRequest>()
                .Event<OpenFullscreenWindowRequest>()
                .Event<OpenOverlayWindowRequest>()
                .Event<CloseMainWindowRequest>()
                .Event<CloseSecondWindowRequest>();
            TestW.Initialize();

            _controller = new WindowsController<TestWorld>();
            TestW.SetResource(_controller);
        }

        [TearDown]
        public void TearDown()
        {
            if (_requestSystemInitialized)
                _requestSystem.Destroy();

            _controller?.Dispose();

            if (TestW.Status != WorldStatus.NotCreated)
                TestW.Destroy();
        }

        [Test]
        public void RegisterWindow_ThrowsForDuplicateWindow()
        {
            RegisterMainWindow();

            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterWindow<MainWindow, TestInput, TestWindowShellView>(
                    static () => new TestWindowShellView());
            });
        }

        [Test]
        public void RegisterRequest_ThrowsWhenWindowIsMissing()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            });
        }

        [Test]
        public void RegisterRequest_ThrowsForDuplicateRequest()
        {
            RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            });
        }

        [Test]
        public void RegisterRequest_AfterRequestSystemInit_InitializesReceiver()
        {
            RegisterMainWindow();
            InitializeRequestSystem();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(5)));
            _requestSystem.Update();

            var viewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();
            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
            Assert.That(viewModel.LastInput, Is.EqualTo(5));
        }

        [Test]
        public void OpenRequest_CreatesViewModelAndBindsSlot()
        {
            var shell = RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(42)));
            _requestSystem.Update();

            var viewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();
            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
            Assert.That(viewModel.LastInput, Is.EqualTo(42));
            Assert.That(viewModel.ApplyInputCount, Is.EqualTo(1));
            Assert.That(shell.GetSlot<MainHeaderSlot>().ViewModel, Is.SameAs(viewModel));
            Assert.That(shell.ShowCount, Is.EqualTo(1));
            Assert.That(shell.Ordering.Layer, Is.EqualTo(EcsWindowLayer.Popup));
            Assert.That(shell.Ordering.OrderInLayer, Is.EqualTo(2));
        }

        [Test]
        public void SecondOpen_ReusesPersistentViewModelAndAppliesInputAgain()
        {
            RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterCloseRequest<MainWindow, CloseMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            var firstViewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();

            TestW.SendEvent(new CloseMainWindowRequest());
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(2)));
            _requestSystem.Update();
            var secondViewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();

            Assert.That(secondViewModel, Is.SameAs(firstViewModel));
            Assert.That(secondViewModel.LastInput, Is.EqualTo(2));
            Assert.That(secondViewModel.ApplyInputCount, Is.EqualTo(2));
        }

        [Test]
        public void Close_DeinitializesSlotsButKeepsViewModelAlive()
        {
            var shell = RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterCloseRequest<MainWindow, CloseMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(7)));
            _requestSystem.Update();
            var viewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();

            TestW.SendEvent(new CloseMainWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(shell.GetSlot<MainHeaderSlot>().ViewModel, Is.Null);
            Assert.That(shell.GetSlot<MainHeaderSlot>().DeinitializeCount, Is.EqualTo(1));
            Assert.That(viewModel.DisposeCount, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_DisposesShellAndPersistentViewModels()
        {
            var shell = RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            var viewModel = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();

            _controller.Dispose();

            Assert.That(shell.IsDisposed, Is.True);
            Assert.That(viewModel.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterViewModel_ThrowsForDuplicateWindowSlot()
        {
            RegisterMainWindow();

            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterViewModel<MainWindow, TestInput, MainHeaderSlot, TestHeaderViewModel>(
                    static _ => new TestHeaderViewModel(),
                    ApplyMainHeaderInput);
            });
        }

        [Test]
        public void Open_ThrowsWhenShellHasNoRegisteredSlot()
        {
            _controller.RegisterWindow<MainWindow, TestInput, TestWindowShellView>(
                static () => new TestWindowShellView());
            RegisterMainHeaderViewModel();

            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.Open<MainWindow, TestInput>(new TestInput(1));
            });
        }

        [Test]
        public void Window_ComposesViewModelsFromSeparateRegistrations()
        {
            var shell = new TestWindowShellView();
            shell.AddSlot<MainHeaderSlot>();
            shell.AddSlot<MainItemsSlot>();

            _controller.RegisterWindow<MainWindow, TestInput, TestWindowShellView>(
                () => shell);
            RegisterMainHeaderViewModel();
            RegisterMainItemsViewModel();

            _controller.Open<MainWindow, TestInput>(new TestInput(9));

            var header = _controller.GetViewModel<MainWindow, MainHeaderSlot, TestHeaderViewModel>();
            var items = _controller.GetViewModel<MainWindow, MainItemsSlot, TestItemsViewModel>();
            Assert.That(shell.GetSlot<MainHeaderSlot>().ViewModel, Is.SameAs(header));
            Assert.That(shell.GetSlot<MainItemsSlot>().ViewModel, Is.SameAs(items));
            Assert.That(header.LastInput, Is.EqualTo(9));
            Assert.That(items.OwnerEntityId, Is.EqualTo(9));
        }

        [Test]
        public void NoDataOpenRequest_ShowsWindow()
        {
            var shell = RegisterSecondWindow();
            _controller.RegisterOpenRequest<SecondWindow, EcsWindowNoData, OpenSecondWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenSecondWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<SecondWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
            Assert.That(shell.ShowCount, Is.EqualTo(1));
        }

        [Test]
        public void CloseRequest_HidesSpecificWindow()
        {
            var shell = RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterCloseRequest<MainWindow, CloseMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(7)));
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new CloseMainWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(shell.HideCount, Is.EqualTo(1));
        }

        [Test]
        public void CloseTop_ClosesTopPopupAndFocusesPreviousPopup()
        {
            var first = RegisterMainWindow();
            var second = RegisterSecondWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterOpenRequest<SecondWindow, EcsWindowNoData, OpenSecondWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new OpenSecondWindowRequest());
            _requestSystem.Update();
            TestW.Tick();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewBlurred));
            Assert.That(_controller.GetState<SecondWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));

            TestW.SendEvent(new CloseTopEcsWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
            Assert.That(_controller.GetState<SecondWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(first.FocusCount, Is.EqualTo(2));
            Assert.That(second.HideCount, Is.EqualTo(1));
        }

        [Test]
        public void CloseAll_ClosesVisibleWindows()
        {
            RegisterMainWindow();
            RegisterSecondWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterOpenRequest<SecondWindow, EcsWindowNoData, OpenSecondWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            TestW.SendEvent(new OpenSecondWindowRequest());
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new CloseAllEcsWindowsRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(_controller.GetState<SecondWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
        }

        [Test]
        public void FullscreenOpen_ClosesPopup()
        {
            RegisterMainWindow();
            RegisterFullscreenWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterOpenRequest<FullscreenWindow, EcsWindowNoData, OpenFullscreenWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new OpenFullscreenWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<MainWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(_controller.GetState<FullscreenWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
        }

        [Test]
        public void OverlayOpen_ClosesFullscreen()
        {
            RegisterFullscreenWindow();
            RegisterOverlayWindow();
            _controller.RegisterOpenRequest<FullscreenWindow, EcsWindowNoData, OpenFullscreenWindowRequest>();
            _controller.RegisterOpenRequest<OverlayWindow, EcsWindowNoData, OpenOverlayWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenFullscreenWindowRequest());
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new OpenOverlayWindowRequest());
            _requestSystem.Update();

            Assert.That(_controller.GetState<FullscreenWindow>(), Is.EqualTo(EcsWindowState.ViewHidden));
            Assert.That(_controller.GetState<OverlayWindow>(), Is.EqualTo(EcsWindowState.ViewFocused));
        }

        [Test]
        public void RegisterPersistentWindow_ThrowsForMissingSortOrder()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterWindow<PersistentAWindow, EcsWindowNoData, TestWindowShellView>(
                    static () => new TestWindowShellView(),
                    EcsWindowLayer.Persistent);
            });
        }

        [Test]
        public void RegisterPersistentWindow_ThrowsForDuplicateSortOrder()
        {
            _controller.RegisterWindow<PersistentAWindow, EcsWindowNoData, TestWindowShellView>(
                static () => new TestWindowShellView(),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 10);

            Assert.Throws<InvalidOperationException>(() =>
            {
                _controller.RegisterWindow<PersistentBWindow, EcsWindowNoData, TestWindowShellView>(
                    static () => new TestWindowShellView(),
                    EcsWindowLayer.Persistent,
                    persistentSortOrder: 10);
            });
        }

        [Test]
        public void PresentationActiveRequest_UpdatesVisibleShell()
        {
            var shell = RegisterMainWindow();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            InitializeRequestSystem();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            TestW.Tick();

            TestW.SendEvent(new SetAllEcsWindowsPresentationActiveRequest(false));
            _requestSystem.Update();

            Assert.That(shell.PresentationActive, Is.False);
        }

        [Test]
        public void PresentationBridge_SyncsOnlyWhileWindowIsActive()
        {
            RegisterMainWindow(registerHeaderViewModel: false);
            RegisterMainItemsViewModel();
            _controller.RegisterOpenRequest<MainWindow, TestInput, OpenMainWindowRequest>();
            _controller.RegisterCloseRequest<MainWindow, CloseMainWindowRequest>();
            InitializeRequestSystem();

            var bridge = new TestBridgeSystem();
            bridge.Init();
            bridge.Update();

            TestW.SendEvent(new OpenMainWindowRequest(new TestInput(1)));
            _requestSystem.Update();
            bridge.Update();
            TestW.Tick();

            var viewModel = _controller.GetViewModel<MainWindow, MainItemsSlot, TestItemsViewModel>();
            Assert.That(viewModel.SyncCount, Is.EqualTo(1));

            TestW.SendEvent(new CloseMainWindowRequest());
            _requestSystem.Update();
            bridge.Update();

            Assert.That(viewModel.SyncCount, Is.EqualTo(1));
        }

        [Test]
        public void RequestSystemInit_ThrowsWhenWindowsControllerResourceIsMissing()
        {
            if (MissingW.Status != WorldStatus.NotCreated)
                MissingW.Destroy();

            MissingW.Create(WorldConfig.Default());
            MissingW.Types()
                .Event<CloseTopEcsWindowRequest>()
                .Event<CloseAllEcsWindowsRequest>()
                .Event<SetAllEcsWindowsPresentationActiveRequest>();
            MissingW.Initialize();

            try
            {
                Assert.Catch<Exception>(() =>
                {
                    new EcsWindowRequestSystem<MissingWorld>().Init();
                });
            }
            finally
            {
                MissingW.Destroy();
            }
        }

        private TestWindowShellView RegisterMainWindow(bool registerHeaderViewModel = true)
        {
            var shell = new TestWindowShellView();
            shell.AddSlot<MainHeaderSlot>();
            shell.AddSlot<MainItemsSlot>();

            _controller.RegisterWindow<MainWindow, TestInput, TestWindowShellView>(
                () => shell,
                EcsWindowLayer.Popup);

            if (registerHeaderViewModel)
                RegisterMainHeaderViewModel();

            return shell;
        }

        private TestWindowShellView RegisterSecondWindow()
        {
            var shell = new TestWindowShellView();
            _controller.RegisterWindow<SecondWindow, EcsWindowNoData, TestWindowShellView>(
                () => shell,
                EcsWindowLayer.Popup);
            return shell;
        }

        private TestWindowShellView RegisterFullscreenWindow()
        {
            var shell = new TestWindowShellView();
            _controller.RegisterWindow<FullscreenWindow, EcsWindowNoData, TestWindowShellView>(
                () => shell,
                EcsWindowLayer.Fullscreen);
            return shell;
        }

        private TestWindowShellView RegisterOverlayWindow()
        {
            var shell = new TestWindowShellView();
            _controller.RegisterWindow<OverlayWindow, EcsWindowNoData, TestWindowShellView>(
                () => shell,
                EcsWindowLayer.Overlay);
            return shell;
        }

        private void RegisterMainHeaderViewModel()
        {
            _controller.RegisterViewModel<MainWindow, TestInput, MainHeaderSlot, TestHeaderViewModel>(
                static _ => new TestHeaderViewModel(),
                ApplyMainHeaderInput);
        }

        private void RegisterMainItemsViewModel()
        {
            _controller.RegisterViewModel<MainWindow, TestInput, MainItemsSlot, TestItemsViewModel>(
                static _ => new TestItemsViewModel(),
                ApplyMainItemsInput);
        }

        private void InitializeRequestSystem()
        {
            _requestSystem = new EcsWindowRequestSystem<TestWorld>();
            _requestSystem.Init();
            _requestSystemInitialized = true;
        }

        private static void ApplyMainHeaderInput(
            TestHeaderViewModel viewModel,
            in EcsWindowContext<TestWorld, MainWindow, TestInput> context)
        {
            viewModel.ApplyInput(context.Input.Value);
        }

        private static void ApplyMainItemsInput(
            TestItemsViewModel viewModel,
            in EcsWindowContext<TestWorld, MainWindow, TestInput> context)
        {
            viewModel.ApplyInput(context.Input.Value);
        }

        private struct TestWorld : IWorldType
        {
        }

        private abstract class TestW : World<TestWorld>
        {
        }

        private struct MissingWorld : IWorldType
        {
        }

        private abstract class MissingW : World<MissingWorld>
        {
        }

        private struct MainWindow : IEcsWindow
        {
        }

        private struct SecondWindow : IEcsWindow
        {
        }

        private struct FullscreenWindow : IEcsWindow
        {
        }

        private struct OverlayWindow : IEcsWindow
        {
        }

        private struct PersistentAWindow : IEcsWindow
        {
        }

        private struct PersistentBWindow : IEcsWindow
        {
        }

        private struct MainHeaderSlot : IEcsWindowSlot
        {
        }

        private struct MainItemsSlot : IEcsWindowSlot
        {
        }

        private readonly struct TestInput
        {
            public readonly int Value;

            public TestInput(int value)
            {
                Value = value;
            }
        }

        private readonly struct OpenMainWindowRequest : IEcsWindowOpenRequest<MainWindow, TestInput>
        {
            public OpenMainWindowRequest(TestInput input)
            {
                Input = input;
            }

            public TestInput Input { get; }
        }

        private readonly struct OpenSecondWindowRequest : IEcsWindowOpenRequest<SecondWindow, EcsWindowNoData>
        {
            public EcsWindowNoData Input => default;
        }

        private readonly struct OpenFullscreenWindowRequest : IEcsWindowOpenRequest<FullscreenWindow, EcsWindowNoData>
        {
            public EcsWindowNoData Input => default;
        }

        private readonly struct OpenOverlayWindowRequest : IEcsWindowOpenRequest<OverlayWindow, EcsWindowNoData>
        {
            public EcsWindowNoData Input => default;
        }

        private readonly struct CloseMainWindowRequest : IEcsWindowCloseRequest<MainWindow>
        {
        }

        private readonly struct CloseSecondWindowRequest : IEcsWindowCloseRequest<SecondWindow>
        {
        }

        private sealed class TestWindowShellView : IEcsWindowShellView
        {
            private readonly Dictionary<Type, TestAspidView> _slotsByType = new();

            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }
            public int FocusCount { get; private set; }
            public int BlurCount { get; private set; }
            public bool IsDisposed { get; private set; }
            public bool PresentationActive { get; private set; } = true;
            public EcsWindowOrdering Ordering { get; private set; }

            public TestAspidView GetSlot<TSlot>()
                where TSlot : struct, IEcsWindowSlot
            {
                return _slotsByType[typeof(TSlot)];
            }

            public void AddSlot<TSlot>()
                where TSlot : struct, IEcsWindowSlot
            {
                _slotsByType.Add(typeof(TSlot), new TestAspidView());
            }

            public void SetDrawOrder(EcsWindowOrdering order)
            {
                Ordering = order;
            }

            public UniTask ShowAsync(CancellationToken ct)
            {
                ShowCount++;
                return UniTask.CompletedTask;
            }

            public UniTask HideAsync(CancellationToken ct, bool isInstant = false)
            {
                HideCount++;
                return UniTask.CompletedTask;
            }

            public void SetPresentationActive(bool isActive)
            {
                PresentationActive = isActive;
            }

            public void Focus()
            {
                FocusCount++;
            }

            public void Blur()
            {
                BlurCount++;
            }

            public void Bind(Type slotType, IViewModel viewModel)
            {
                if (!_slotsByType.TryGetValue(slotType, out var view))
                    throw new InvalidOperationException($"Missing test slot `{slotType.FullName}`.");

                view.Initialize(viewModel);
            }

            public void Unbind(Type slotType)
            {
                if (!_slotsByType.TryGetValue(slotType, out var view))
                    throw new InvalidOperationException($"Missing test slot `{slotType.FullName}`.");

                view.Deinitialize();
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private sealed class TestAspidView : IView
        {
            public IViewModel ViewModel { get; private set; }
            public int InitializeCount { get; private set; }
            public int DeinitializeCount { get; private set; }

            public void Initialize(IViewModel viewModel)
            {
                if (ViewModel != null)
                    throw new InvalidOperationException("Test view is already initialized.");

                ViewModel = viewModel;
                InitializeCount++;
            }

            public void Deinitialize()
            {
                if (ViewModel == null)
                    throw new InvalidOperationException("Test view is not initialized.");

                ViewModel = null;
                DeinitializeCount++;
            }
        }

        private sealed class TestHeaderViewModel : IViewModel, IDisposable
        {
            public int LastInput { get; private set; }
            public int ApplyInputCount { get; private set; }
            public int DisposeCount { get; private set; }

            public void ApplyInput(int value)
            {
                LastInput = value;
                ApplyInputCount++;
            }

            public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
            {
                return default;
            }

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        private sealed class TestItemsViewModel : IViewModel, IDisposable
        {
            public int OwnerEntityId { get; private set; }
            public int SyncCount { get; private set; }
            public int DisposeCount { get; private set; }

            public void ApplyInput(int ownerEntityId)
            {
                OwnerEntityId = ownerEntityId;
            }

            public void SyncPresentation()
            {
                SyncCount++;
            }

            public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
            {
                return default;
            }

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        private sealed class TestBridgeSystem
            : EcsWindowPresentationBridgeSystem<TestWorld, MainWindow, MainItemsSlot, TestItemsViewModel>
        {
            protected override void SyncPresentation(TestItemsViewModel viewModel)
            {
                viewModel.SyncPresentation();
            }
        }
    }
}
