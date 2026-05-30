using System;
using System.Collections.Generic;
using System.Threading;
using Aspid.MVVM;
using Aspid.StaticEcs;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Windows
{
    public sealed class WindowsController<TWorld> : IResource, IDisposable
        where TWorld : struct, IWorldType
    {
        private const int POPUP_ORDER_INCREMENT = 2;

        private readonly Dictionary<Type, WindowEntry> _windowsByType = new();
        private readonly Dictionary<Type, RequestReceiver> _requestReceiversByType = new();
        private readonly List<WindowEntry> _windows = new();
        private readonly List<RequestReceiver> _requestReceivers = new();
        private readonly List<PopupStackItem> _popupStack = new();
        private readonly List<PersistentStackItem> _persistentStack = new();
        private readonly List<WindowEntry> _closeableStack = new();
        private readonly CancellationTokenSource _disposeCts = new();

        private WindowEntry _fullscreenWindow;
        private WindowEntry _overlayWindow;
        private bool _requestReceiversInitialized;
        private bool _isPresentationActive = true;
        private bool _isDisposed;

        public void RegisterWindow<TWindow, TInput, TView>(
            Func<TView> viewFactory,
            EcsWindowLayer layer = EcsWindowLayer.Popup,
            int? persistentSortOrder = null,
            bool? canBeClosedByEscape = null)
            where TWindow : struct, IEcsWindow
            where TView : class, IEcsWindowShellView
        {
            ThrowIfDisposed();

            if (viewFactory == null)
                throw new ArgumentNullException(nameof(viewFactory));

            var windowType = typeof(TWindow);
            if (_windowsByType.ContainsKey(windowType))
                throw new InvalidOperationException($"ECS window `{windowType.FullName}` is already registered.");

            ValidatePersistentSortOrder(windowType, layer, persistentSortOrder);

            IEcsWindowShellView CreateShell()
            {
                var shell = viewFactory();
                if (shell == null)
                    throw new InvalidOperationException($"Factory returned null shell view for ECS window `{windowType.FullName}`.");

                return shell;
            }

            var entry = new WindowEntry(
                windowType,
                typeof(TInput),
                CreateShell,
                layer,
                persistentSortOrder,
                canBeClosedByEscape ?? layer is EcsWindowLayer.Popup or EcsWindowLayer.Fullscreen);

            _windowsByType.Add(windowType, entry);
            _windows.Add(entry);
        }

        public void RegisterViewModel<TWindow, TInput, TSlot, TViewModel>(
            Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> factory,
            EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> applyInput)
            where TWindow : struct, IEcsWindow
            where TSlot : struct, IEcsWindowSlot
            where TViewModel : class, IViewModel
        {
            ThrowIfDisposed();

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (applyInput == null)
                throw new ArgumentNullException(nameof(applyInput));

            var entry = GetEntry(typeof(TWindow));
            if (entry.InputType != typeof(TInput))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` expects input `{entry.InputType.FullName}`, not `{typeof(TInput).FullName}`.");
            }

            var slotType = typeof(TSlot);
            if (entry.SlotsByType.ContainsKey(slotType))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` already has a ViewModel registered for slot `{slotType.FullName}`.");
            }

            var slot = new SlotEntry<TWindow, TInput, TSlot, TViewModel>(factory, applyInput);
            entry.SlotsByType.Add(slotType, slot);
            entry.Slots.Add(slot);
        }

        public void RegisterLinkedViewModel<TWindow, TInput, TSlot, TViewModel>(
            Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> factory,
            Func<EcsWindowContext<TWorld, TWindow, TInput>, EntityGID> entityResolver,
            EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> applyInput)
            where TWindow : struct, IEcsWindow
            where TSlot : struct, IEcsWindowSlot
            where TViewModel : class, IViewModel
        {
            ThrowIfDisposed();

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (entityResolver == null)
                throw new ArgumentNullException(nameof(entityResolver));

            if (applyInput == null)
                throw new ArgumentNullException(nameof(applyInput));

            var entry = GetEntry(typeof(TWindow));
            if (entry.InputType != typeof(TInput))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` expects input `{entry.InputType.FullName}`, not `{typeof(TInput).FullName}`.");
            }

            var slotType = typeof(TSlot);
            if (entry.SlotsByType.ContainsKey(slotType))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` already has a ViewModel registered for slot `{slotType.FullName}`.");
            }

            var slot = new LinkedSlotEntry<TWindow, TInput, TSlot, TViewModel>(
                factory,
                entityResolver,
                applyInput);
            entry.SlotsByType.Add(slotType, slot);
            entry.Slots.Add(slot);
        }

        public void RegisterOpenRequest<TWindow, TInput, TOpenRequest>()
            where TWindow : struct, IEcsWindow
            where TOpenRequest : struct, IEcsWindowOpenRequest<TWindow, TInput>
        {
            ThrowIfDisposed();

            var entry = GetEntry(typeof(TWindow));
            if (entry.InputType != typeof(TInput))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` expects input `{entry.InputType.FullName}`, not `{typeof(TInput).FullName}`.");
            }

            RegisterRequestReceiver(new OpenRequestReceiver<TWindow, TInput, TOpenRequest>());
        }

        public void RegisterCloseRequest<TWindow, TCloseRequest>()
            where TWindow : struct, IEcsWindow
            where TCloseRequest : struct, IEcsWindowCloseRequest<TWindow>
        {
            ThrowIfDisposed();

            GetEntry(typeof(TWindow));
            RegisterRequestReceiver(new CloseRequestReceiver<TWindow, TCloseRequest>());
        }

        public TViewModel GetViewModel<TWindow, TSlot, TViewModel>()
            where TWindow : struct, IEcsWindow
            where TSlot : struct, IEcsWindowSlot
            where TViewModel : class, IViewModel
        {
            ThrowIfDisposed();

            var entry = GetEntry(typeof(TWindow));
            var slot = entry.GetSlot(typeof(TSlot));
            if (slot.ViewModel == null)
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` slot `{typeof(TSlot).FullName}` ViewModel has not been created yet.");
            }

            if (slot.ViewModel is TViewModel viewModel)
                return viewModel;

            throw new InvalidOperationException(
                $"ECS window `{typeof(TWindow).FullName}` slot `{typeof(TSlot).FullName}` uses ViewModel `{slot.ViewModelType.FullName}`, not `{typeof(TViewModel).FullName}`.");
        }

        public EcsWindowState GetState<TWindow>()
            where TWindow : struct, IEcsWindow
        {
            ThrowIfDisposed();

            return GetEntry(typeof(TWindow)).State;
        }

        public bool IsWindowActive<TWindow>()
            where TWindow : struct, IEcsWindow
        {
            ThrowIfDisposed();

            var state = GetEntry(typeof(TWindow)).State;
            return state == EcsWindowState.ViewFocused || state == EcsWindowState.ViewBlurred;
        }

        public bool Open<TWindow, TInput>(in TInput input)
            where TWindow : struct, IEcsWindow
        {
            ThrowIfDisposed();

            var entry = GetEntry(typeof(TWindow));
            if (entry.InputType != typeof(TInput))
            {
                throw new InvalidOperationException(
                    $"ECS window `{typeof(TWindow).FullName}` expects input `{entry.InputType.FullName}`, not `{typeof(TInput).FullName}`.");
            }

            if (entry.State != EcsWindowState.ViewHidden)
                return false;

            entry.CreateShellIfMissing();
            entry.Shell.SetPresentationActive(_isPresentationActive);
            ApplyOpenInput<TWindow, TInput>(entry, input);
            BindSlots(entry);

            var ordering = Push(entry);
            RunLifecycleAsync(entry, ordering).Forget();
            return true;
        }

        public bool TryClose<TWindow>()
            where TWindow : struct, IEcsWindow
        {
            ThrowIfDisposed();

            return TryClose(GetEntry(typeof(TWindow)));
        }

        public bool TryCloseTopClosable()
        {
            ThrowIfDisposed();

            if (_closeableStack.Count == 0)
                return false;

            RequestClose(_closeableStack[^1]);
            return true;
        }

        public bool TryCloseAll()
        {
            ThrowIfDisposed();

            var closedAny = false;
            for (var i = 0; i < _windows.Count; i++)
                closedAny |= TryClose(_windows[i]);

            return closedAny;
        }

        public void SetAllWindowsPresentationActive(bool isActive)
        {
            ThrowIfDisposed();

            _isPresentationActive = isActive;
            for (var i = 0; i < _windows.Count; i++)
                _windows[i].Shell?.SetPresentationActive(isActive);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _disposeCts.Cancel();

            for (var i = 0; i < _windows.Count; i++)
                DisposeWindow(_windows[i]);

            _closeableStack.Clear();
            _popupStack.Clear();
            _persistentStack.Clear();
            _fullscreenWindow = null;
            _overlayWindow = null;
            _disposeCts.Dispose();
        }

        internal void InitializeRequestReceivers()
        {
            ThrowIfDisposed();

            if (_requestReceiversInitialized)
                throw new InvalidOperationException($"{nameof(WindowsController<TWorld>)} request receivers are already initialized.");

            for (var i = 0; i < _requestReceivers.Count; i++)
                _requestReceivers[i].Initialize();

            _requestReceiversInitialized = true;
        }

        internal void DestroyRequestReceivers()
        {
            for (var i = 0; i < _requestReceivers.Count; i++)
                _requestReceivers[i].Destroy();

            _requestReceiversInitialized = false;
        }

        internal void ProcessRequestReceivers()
        {
            ThrowIfDisposed();

            for (var i = 0; i < _requestReceivers.Count; i++)
                _requestReceivers[i].Drain(this);
        }

        private async UniTask RunLifecycleAsync(WindowEntry entry, EcsWindowOrdering ordering)
        {
            try
            {
                entry.CloseIntent = new UniTaskCompletionSource();
                entry.Shell.SetDrawOrder(ordering);

                entry.State = EcsWindowState.ViewShowing;
                await entry.Shell.ShowAsync(_disposeCts.Token);
                _disposeCts.Token.ThrowIfCancellationRequested();

                entry.State = EcsWindowState.ViewFocused;
                entry.Shell.Focus();

                await entry.CloseIntent.Task.AttachExternalCancellation(_disposeCts.Token);

                entry.State = EcsWindowState.ViewHiding;
                entry.Shell.Blur();
                await entry.Shell.HideAsync(_disposeCts.Token);
                _disposeCts.Token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                UnbindSlots(entry);
                entry.CloseIntent = null;
                entry.State = EcsWindowState.ViewHidden;
                Pop(entry);
            }
        }

        private void ApplyOpenInput<TWindow, TInput>(WindowEntry entry, TInput input)
            where TWindow : struct, IEcsWindow
        {
            if (entry.Slots.Count == 0)
                return;

            var boxedInput = (object)input;
            for (var i = 0; i < entry.Slots.Count; i++)
                entry.Slots[i].CreateIfMissingAndApply(this, entry, boxedInput);
        }

        private void BindSlots(WindowEntry entry)
        {
            try
            {
                for (var i = 0; i < entry.Slots.Count; i++)
                {
                    var slot = entry.Slots[i];
                    entry.Shell.Bind(slot.SlotType, slot.ViewModel);
                    slot.IsBound = true;
                }
            }
            catch
            {
                UnbindSlots(entry);
                throw;
            }
        }

        private void UnbindSlots(WindowEntry entry)
        {
            if (entry.Shell == null)
                return;

            for (var i = 0; i < entry.Slots.Count; i++)
            {
                var slot = entry.Slots[i];
                if (!slot.IsBound)
                    continue;

                entry.Shell.Unbind(slot.SlotType);
                slot.IsBound = false;
            }
        }

        private bool TryClose(WindowEntry entry)
        {
            if (entry.State == EcsWindowState.ViewHidden)
                return false;

            RequestClose(entry);
            return true;
        }

        private void RequestClose(WindowEntry entry)
        {
            entry.CloseIntent?.TrySetResult();
        }

        private EcsWindowOrdering Push(WindowEntry entry)
        {
            return entry.Layer switch
            {
                EcsWindowLayer.Popup => PushPopup(entry),
                EcsWindowLayer.Fullscreen => PushFullscreen(entry),
                EcsWindowLayer.Persistent => PushPersistent(entry),
                EcsWindowLayer.Overlay => PushOverlay(entry),
                _ => throw new ArgumentOutOfRangeException(nameof(entry.Layer), entry.Layer, null)
            };
        }

        private EcsWindowOrdering PushPopup(WindowEntry entry)
        {
            var order = POPUP_ORDER_INCREMENT;

            if (_popupStack.Count > 0)
            {
                var previous = _popupStack[^1];
                Blur(previous.Entry);
                order = previous.OrderInLayer + POPUP_ORDER_INCREMENT;
            }

            BlurPersistentWindows();
            _popupStack.Add(new PopupStackItem(entry, order));
            AddCloseableIfNeeded(entry);
            return new EcsWindowOrdering(EcsWindowLayer.Popup, order);
        }

        private EcsWindowOrdering PushFullscreen(WindowEntry entry)
        {
            if (_fullscreenWindow != null && !ReferenceEquals(_fullscreenWindow, entry))
                RequestClose(_fullscreenWindow);

            ClosePopupWindows();
            BlurPersistentWindows();
            _fullscreenWindow = entry;
            AddCloseableIfNeeded(entry);
            return new EcsWindowOrdering(EcsWindowLayer.Fullscreen, 0);
        }

        private EcsWindowOrdering PushPersistent(WindowEntry entry)
        {
            var sortOrder = GetPersistentSortOrderOrThrow(entry);
            ValidateUniqueActivePersistentSortOrder(entry, sortOrder);

            _persistentStack.Add(new PersistentStackItem(entry, sortOrder));
            AddCloseableIfNeeded(entry);
            return new EcsWindowOrdering(EcsWindowLayer.Persistent, sortOrder);
        }

        private EcsWindowOrdering PushOverlay(WindowEntry entry)
        {
            if (_overlayWindow != null && !ReferenceEquals(_overlayWindow, entry))
                RequestClose(_overlayWindow);

            ClosePopupWindows();

            if (_fullscreenWindow != null)
                RequestClose(_fullscreenWindow);

            _overlayWindow = entry;
            AddCloseableIfNeeded(entry);
            return new EcsWindowOrdering(EcsWindowLayer.Overlay, 1);
        }

        private void Pop(WindowEntry entry)
        {
            switch (entry.Layer)
            {
                case EcsWindowLayer.Popup:
                    PopPopup(entry);
                    break;
                case EcsWindowLayer.Fullscreen:
                    PopFullscreen(entry);
                    break;
                case EcsWindowLayer.Persistent:
                    RemovePersistent(entry);
                    break;
                case EcsWindowLayer.Overlay:
                    PopOverlay(entry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry.Layer), entry.Layer, null);
            }
        }

        private void PopPopup(WindowEntry entry)
        {
            RemovePopup(entry);
            RemoveCloseable(entry);

            if (_popupStack.Count > 0)
            {
                Focus(_popupStack[^1].Entry);
                return;
            }

            FocusPersistentWindows();
        }

        private void PopFullscreen(WindowEntry entry)
        {
            if (ReferenceEquals(_fullscreenWindow, entry))
                _fullscreenWindow = null;

            RemoveCloseable(entry);
            FocusPersistentWindows();
        }

        private void RemovePersistent(WindowEntry entry)
        {
            for (var i = 0; i < _persistentStack.Count; i++)
            {
                if (!ReferenceEquals(_persistentStack[i].Entry, entry))
                    continue;

                _persistentStack.RemoveAt(i);
                break;
            }

            RemoveCloseable(entry);
        }

        private void PopOverlay(WindowEntry entry)
        {
            if (ReferenceEquals(_overlayWindow, entry))
                _overlayWindow = null;

            RemoveCloseable(entry);
        }

        private void ClosePopupWindows()
        {
            for (var i = _popupStack.Count - 1; i >= 0; i--)
                RequestClose(_popupStack[i].Entry);
        }

        private void BlurPersistentWindows()
        {
            for (var i = 0; i < _persistentStack.Count; i++)
            {
                var entry = _persistentStack[i].Entry;
                if (entry.State == EcsWindowState.ViewFocused)
                    Blur(entry);
            }
        }

        private void FocusPersistentWindows()
        {
            for (var i = 0; i < _persistentStack.Count; i++)
                Focus(_persistentStack[i].Entry);
        }

        private void Focus(WindowEntry entry)
        {
            if (entry.State == EcsWindowState.ViewHidden)
                return;

            if (entry.Shell == null)
                throw new InvalidOperationException($"ECS window `{entry.WindowType.FullName}` has no shell view to focus.");

            entry.State = EcsWindowState.ViewFocused;
            entry.Shell.Focus();
        }

        private void Blur(WindowEntry entry)
        {
            if (entry.State == EcsWindowState.ViewHidden)
                return;

            if (entry.Shell == null)
                throw new InvalidOperationException($"ECS window `{entry.WindowType.FullName}` has no shell view to blur.");

            entry.State = EcsWindowState.ViewBlurred;
            entry.Shell.Blur();
        }

        private void RemovePopup(WindowEntry entry)
        {
            for (var i = 0; i < _popupStack.Count; i++)
            {
                if (!ReferenceEquals(_popupStack[i].Entry, entry))
                    continue;

                _popupStack.RemoveAt(i);
                return;
            }
        }

        private void AddCloseableIfNeeded(WindowEntry entry)
        {
            if (entry.CanBeClosedByEscape)
                _closeableStack.Add(entry);
        }

        private void RemoveCloseable(WindowEntry entry)
        {
            for (var i = 0; i < _closeableStack.Count; i++)
            {
                if (!ReferenceEquals(_closeableStack[i], entry))
                    continue;

                _closeableStack.RemoveAt(i);
                return;
            }
        }

        private void RegisterRequestReceiver(RequestReceiver receiver)
        {
            if (_requestReceiversByType.ContainsKey(receiver.RequestType))
                throw new InvalidOperationException($"ECS window request `{receiver.RequestType.FullName}` is already registered.");

            if (_requestReceiversInitialized)
                receiver.Initialize();

            _requestReceiversByType.Add(receiver.RequestType, receiver);
            _requestReceivers.Add(receiver);
        }

        private WindowEntry GetEntry(Type windowType)
        {
            if (_windowsByType.TryGetValue(windowType, out var entry))
                return entry;

            throw new InvalidOperationException($"ECS window `{windowType.FullName}` is not registered.");
        }

        private void ValidatePersistentSortOrder(Type windowType, EcsWindowLayer layer, int? persistentSortOrder)
        {
            if (layer != EcsWindowLayer.Persistent)
                return;

            if (!persistentSortOrder.HasValue)
            {
                throw new InvalidOperationException(
                    $"Persistent ECS window `{windowType.FullName}` must declare a unique persistent sort order.");
            }

            var sortOrder = persistentSortOrder.Value;
            for (var i = 0; i < _windows.Count; i++)
            {
                var existing = _windows[i];
                if (existing.Layer != EcsWindowLayer.Persistent || existing.PersistentSortOrder != sortOrder)
                    continue;

                throw new InvalidOperationException(
                    $"Persistent ECS windows `{windowType.FullName}` and `{existing.WindowType.FullName}` share sort order {sortOrder}.");
            }
        }

        private void ValidateUniqueActivePersistentSortOrder(WindowEntry entry, int sortOrder)
        {
            for (var i = 0; i < _persistentStack.Count; i++)
            {
                var existing = _persistentStack[i];
                if (existing.SortOrder != sortOrder || ReferenceEquals(existing.Entry, entry))
                    continue;

                throw new InvalidOperationException(
                    $"Persistent ECS windows `{entry.WindowType.FullName}` and `{existing.Entry.WindowType.FullName}` share sort order {sortOrder}.");
            }
        }

        private static int GetPersistentSortOrderOrThrow(WindowEntry entry)
        {
            if (entry.PersistentSortOrder.HasValue)
                return entry.PersistentSortOrder.Value;

            throw new InvalidOperationException(
                $"ECS window `{entry.WindowType.FullName}` uses {EcsWindowLayer.Persistent} but does not declare a persistent sort order.");
        }

        private void DisposeWindow(WindowEntry entry)
        {
            UnbindSlots(entry);
            entry.Shell?.Dispose();

            for (var i = 0; i < entry.Slots.Count; i++)
                entry.Slots[i].DisposeViewModel();

            entry.State = EcsWindowState.ViewHidden;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(WindowsController<TWorld>));
        }

        private sealed class WindowEntry
        {
            private readonly Func<IEcsWindowShellView> _shellFactory;

            public readonly Type WindowType;
            public readonly Type InputType;
            public readonly EcsWindowLayer Layer;
            public readonly int? PersistentSortOrder;
            public readonly bool CanBeClosedByEscape;
            public readonly Dictionary<Type, SlotEntry> SlotsByType = new();
            public readonly List<SlotEntry> Slots = new();

            public IEcsWindowShellView Shell;
            public EcsWindowState State = EcsWindowState.ViewHidden;
            public UniTaskCompletionSource CloseIntent;

            public WindowEntry(
                Type windowType,
                Type inputType,
                Func<IEcsWindowShellView> shellFactory,
                EcsWindowLayer layer,
                int? persistentSortOrder,
                bool canBeClosedByEscape)
            {
                WindowType = windowType;
                InputType = inputType;
                _shellFactory = shellFactory;
                Layer = layer;
                PersistentSortOrder = persistentSortOrder;
                CanBeClosedByEscape = canBeClosedByEscape;
            }

            public void CreateShellIfMissing()
            {
                Shell ??= _shellFactory();
            }

            public SlotEntry GetSlot(Type slotType)
            {
                if (SlotsByType.TryGetValue(slotType, out var slot))
                    return slot;

                throw new InvalidOperationException(
                    $"ECS window `{WindowType.FullName}` has no ViewModel registration for slot `{slotType.FullName}`.");
            }
        }

        private abstract class SlotEntry
        {
            protected SlotEntry(Type slotType, Type viewModelType)
            {
                SlotType = slotType;
                ViewModelType = viewModelType;
            }

            public readonly Type SlotType;
            public readonly Type ViewModelType;
            public IViewModel ViewModel;
            public bool IsBound;

            public abstract void CreateIfMissingAndApply(
                WindowsController<TWorld> windows,
                WindowEntry entry,
                object input);

            public virtual void DisposeViewModel()
            {
                ViewModel?.DisposeViewModel();
                ViewModel = null;
                IsBound = false;
            }
        }

        private sealed class LinkedSlotEntry<TWindow, TInput, TSlot, TViewModel> : SlotEntry
            where TWindow : struct, IEcsWindow
            where TSlot : struct, IEcsWindowSlot
            where TViewModel : class, IViewModel
        {
            private readonly Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> _factory;
            private readonly Func<EcsWindowContext<TWorld, TWindow, TInput>, EntityGID> _entityResolver;
            private readonly EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> _applyInput;
            private EcsLink<TWorld, TViewModel> _link;

            public LinkedSlotEntry(
                Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> factory,
                Func<EcsWindowContext<TWorld, TWindow, TInput>, EntityGID> entityResolver,
                EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> applyInput)
                : base(typeof(TSlot), typeof(TViewModel))
            {
                _factory = factory;
                _entityResolver = entityResolver;
                _applyInput = applyInput;
            }

            public override void CreateIfMissingAndApply(
                WindowsController<TWorld> windows,
                WindowEntry entry,
                object input)
            {
                if (entry.WindowType != typeof(TWindow) || entry.InputType != typeof(TInput))
                {
                    throw new InvalidOperationException(
                        $"ECS window `{entry.WindowType.FullName}` slot `{SlotType.FullName}` was requested with invalid input type.");
                }

                var context = new EcsWindowContext<TWorld, TWindow, TInput>(windows, (TInput)input);
                if (ViewModel == null)
                {
                    var viewModel = _factory(context);
                    if (viewModel == null)
                    {
                        throw new InvalidOperationException(
                            $"Factory returned null ViewModel `{typeof(TViewModel).FullName}` for ECS window `{entry.WindowType.FullName}` slot `{SlotType.FullName}`.");
                    }

                    ViewModel = viewModel;
                }

                _applyInput((TViewModel)ViewModel, in context);

                var gid = _entityResolver(context);
                if (_link != null && !_link.IsDisposed && _link.EntityGID == gid)
                    return;

                _link?.Dispose();
                var entity = gid.Unpack<TWorld>();
                var registry = World<TWorld>.GetResource<EcsLinkRegistry<TWorld>>();
                _link = registry.Attach(entity, (TViewModel)ViewModel);
            }

            public override void DisposeViewModel()
            {
                _link?.Dispose();
                _link = null;
                base.DisposeViewModel();
            }
        }

        private sealed class SlotEntry<TWindow, TInput, TSlot, TViewModel> : SlotEntry
            where TWindow : struct, IEcsWindow
            where TSlot : struct, IEcsWindowSlot
            where TViewModel : class, IViewModel
        {
            private readonly Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> _factory;
            private readonly EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> _applyInput;

            public SlotEntry(
                Func<EcsWindowContext<TWorld, TWindow, TInput>, TViewModel> factory,
                EcsWindowOpenInputBinding<TWorld, TWindow, TInput, TViewModel> applyInput)
                : base(typeof(TSlot), typeof(TViewModel))
            {
                _factory = factory;
                _applyInput = applyInput;
            }

            public override void CreateIfMissingAndApply(
                WindowsController<TWorld> windows,
                WindowEntry entry,
                object input)
            {
                if (entry.WindowType != typeof(TWindow) || entry.InputType != typeof(TInput))
                {
                    throw new InvalidOperationException(
                        $"ECS window `{entry.WindowType.FullName}` slot `{SlotType.FullName}` was requested with invalid input type.");
                }

                var context = new EcsWindowContext<TWorld, TWindow, TInput>(windows, (TInput)input);
                if (ViewModel == null)
                {
                    var viewModel = _factory(context);
                    if (viewModel == null)
                    {
                        throw new InvalidOperationException(
                            $"Factory returned null ViewModel `{typeof(TViewModel).FullName}` for ECS window `{entry.WindowType.FullName}` slot `{SlotType.FullName}`.");
                    }

                    ViewModel = viewModel;
                }

                _applyInput((TViewModel)ViewModel, in context);
            }
        }

        private readonly struct PopupStackItem
        {
            public readonly WindowEntry Entry;
            public readonly int OrderInLayer;

            public PopupStackItem(WindowEntry entry, int orderInLayer)
            {
                Entry = entry;
                OrderInLayer = orderInLayer;
            }
        }

        private readonly struct PersistentStackItem
        {
            public readonly WindowEntry Entry;
            public readonly int SortOrder;

            public PersistentStackItem(WindowEntry entry, int sortOrder)
            {
                Entry = entry;
                SortOrder = sortOrder;
            }
        }

        private abstract class RequestReceiver
        {
            protected RequestReceiver(Type requestType)
            {
                RequestType = requestType;
            }

            public Type RequestType { get; }

            public abstract void Initialize();

            public abstract void Destroy();

            public abstract void Drain(WindowsController<TWorld> controller);
        }

        private sealed class OpenRequestReceiver<TWindow, TInput, TOpenRequest> : RequestReceiver
            where TWindow : struct, IEcsWindow
            where TOpenRequest : struct, IEcsWindowOpenRequest<TWindow, TInput>
        {
            private EventReceiver<TWorld, TOpenRequest> _receiver;

            public OpenRequestReceiver()
                : base(typeof(TOpenRequest))
            {
            }

            public override void Initialize()
            {
                _receiver = World<TWorld>.RegisterEventReceiver<TOpenRequest>();
            }

            public override void Destroy()
            {
                World<TWorld>.DeleteEventReceiver(ref _receiver);
            }

            public override void Drain(WindowsController<TWorld> controller)
            {
                foreach (var evt in _receiver)
                {
                    ref readonly var request = ref evt.Value;
                    controller.Open<TWindow, TInput>(request.Input);
                }
            }
        }

        private sealed class CloseRequestReceiver<TWindow, TCloseRequest> : RequestReceiver
            where TWindow : struct, IEcsWindow
            where TCloseRequest : struct, IEcsWindowCloseRequest<TWindow>
        {
            private EventReceiver<TWorld, TCloseRequest> _receiver;

            public CloseRequestReceiver()
                : base(typeof(TCloseRequest))
            {
            }

            public override void Initialize()
            {
                _receiver = World<TWorld>.RegisterEventReceiver<TCloseRequest>();
            }

            public override void Destroy()
            {
                World<TWorld>.DeleteEventReceiver(ref _receiver);
            }

            public override void Drain(WindowsController<TWorld> controller)
            {
                foreach (var _ in _receiver)
                    controller.TryClose<TWindow>();
            }
        }
    }
}
