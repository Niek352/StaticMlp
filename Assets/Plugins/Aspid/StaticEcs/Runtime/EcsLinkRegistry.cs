using System;
using System.Collections.Generic;
using Aspid.MVVM;
using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs
{
    public sealed class EcsLinkRegistry<TWorld> : IResource
        where TWorld : struct, IWorldType
    {
        private readonly List<ILink> _links = new();
        private readonly List<IBinding> _bindings = new();
        private readonly Dictionary<LinkKey, ILink> _linksByKey = new();
        private readonly Dictionary<Type, List<ILink>> _linksByViewModelType = new();
        private readonly Dictionary<BindingKey, IBinding> _bindingsByKey = new();

        public EcsLink<TWorld, TViewModel> Create<TViewModel>(
            World<TWorld>.Entity entity,
            Func<EntityGID, TViewModel> factory,
            bool disposeViewModel = true)
            where TViewModel : class, IViewModel
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var gid = entity.GID;
            var viewModelType = typeof(TViewModel);
            var key = new LinkKey(gid, viewModelType);

            if (_linksByKey.ContainsKey(key))
                throw new InvalidOperationException(
                    $"ECS link already exists for entity `{gid.Raw}` and ViewModel `{viewModelType.FullName}`.");

            var viewModel = factory(gid);
            if (viewModel == null)
                throw new InvalidOperationException(
                    $"ECS link factory returned null for ViewModel `{viewModelType.FullName}`.");

            var link = new EcsLink<TWorld, TViewModel>(this, gid, viewModel, disposeViewModel);
            var entry = new LinkEntry<TViewModel>(link);
            AddLink(key, entry);
            ApplyInitial(entry, entity);

            return link;
        }

        public void RegisterComponent<TViewModel, TComponent>(
            EcsComponentBinding<TViewModel, TComponent> binding)
            where TViewModel : class, IViewModel
            where TComponent : struct, IComponent, ITrackableAdded, ITrackableChanged
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            RegisterBinding(new ComponentBinding<TViewModel, TComponent>(binding));
        }

        public void RegisterTag<TViewModel, TTag>(EcsTagBinding<TViewModel, TTag> binding)
            where TViewModel : class, IViewModel
            where TTag : struct, ITag, ITrackableAdded, ITrackableDeleted
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            RegisterBinding(new TagBinding<TViewModel, TTag>(binding));
        }

        public void RegisterMulti<TViewModel, TElement>(
            EcsMultiBinding<TWorld, TViewModel, TElement> binding,
            EcsMultiSyncMode mode = EcsMultiSyncMode.PollLinkedEntities)
            where TViewModel : class, IViewModel
            where TElement : struct, IMultiComponent
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            if (mode != EcsMultiSyncMode.PollLinkedEntities)
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);

            RegisterBinding(new MultiBinding<TViewModel, TElement>(binding));
        }

        internal void Remove<TViewModel>(EcsLink<TWorld, TViewModel> link)
            where TViewModel : class, IViewModel
        {
            var key = new LinkKey(link.EntityGID, typeof(TViewModel));
            if (!_linksByKey.TryGetValue(key, out var entry))
                return;

            Remove(entry);
        }

        internal void Sync()
        {
            RemoveDeadLinks();

            for (var i = 0; i < _bindings.Count; i++)
                _bindings[i].Sync(this);
        }

        private void AddLink(LinkKey key, ILink link)
        {
            _linksByKey.Add(key, link);
            _links.Add(link);

            if (!_linksByViewModelType.TryGetValue(link.ViewModelType, out var links))
            {
                links = new List<ILink>();
                _linksByViewModelType.Add(link.ViewModelType, links);
            }

            links.Add(link);
        }

        private void RegisterBinding(IBinding binding)
        {
            var key = new BindingKey(binding.Kind, binding.ViewModelType, binding.EcsType);

            if (!_bindingsByKey.TryAdd(key, binding))
                throw new InvalidOperationException(
                    $"ECS binding already exists for `{binding.ViewModelType.FullName}` and `{binding.EcsType.FullName}`.");

            _bindings.Add(binding);
            ApplyInitialToExistingLinks(binding);
        }

        private void ApplyInitial(ILink link, World<TWorld>.Entity entity)
        {
            for (var i = 0; i < _bindings.Count; i++)
            {
                var binding = _bindings[i];
                if (binding.ViewModelType == link.ViewModelType)
                    binding.ApplyInitial(link, entity);
            }
        }

        private void ApplyInitialToExistingLinks(IBinding binding)
        {
            if (!_linksByViewModelType.TryGetValue(binding.ViewModelType, out var links))
                return;

            for (var i = links.Count - 1; i >= 0; i--)
            {
                var link = links[i];
                if (!link.EntityGID.TryUnpack<TWorld>(out var entity))
                {
                    Remove(link);
                    continue;
                }

                binding.ApplyInitial(link, entity);
            }
        }

        private void RemoveDeadLinks()
        {
            for (var i = _links.Count - 1; i >= 0; i--)
            {
                var link = _links[i];
                if (link.EntityGID.TryUnpack<TWorld>(out _))
                    continue;

                Remove(link);
            }
        }

        private void Remove(ILink link)
        {
            if (link.IsDisposed)
                return;

            var key = new LinkKey(link.EntityGID, link.ViewModelType);
            if (!_linksByKey.Remove(key))
                return;

            RemoveFromList(_links, link);

            var links = _linksByViewModelType[link.ViewModelType];
            RemoveFromList(links, link);

            if (links.Count == 0)
                _linksByViewModelType.Remove(link.ViewModelType);

            link.DisposeFromRegistry();
        }

        private static void RemoveFromList(List<ILink> links, ILink link)
        {
            for (var i = 0; i < links.Count; i++)
            {
                if (!ReferenceEquals(links[i], link))
                    continue;

                var lastIndex = links.Count - 1;
                links[i] = links[lastIndex];
                links.RemoveAt(lastIndex);
                return;
            }
        }

        private void ApplyComponent<TViewModel, TComponent>(
            EntityGID gid,
            in TComponent component,
            EcsComponentBinding<TViewModel, TComponent> binding)
            where TViewModel : class, IViewModel
            where TComponent : struct, IComponent
        {
            var key = new LinkKey(gid, typeof(TViewModel));
            if (!_linksByKey.TryGetValue(key, out var link))
                return;

            var typedEntry = (LinkEntry<TViewModel>)link;
            binding(typedEntry.Link.ViewModel, in component);
        }

        private void ApplyTag<TViewModel, TTag>(
            EntityGID gid,
            bool isPresent,
            EcsTagBinding<TViewModel, TTag> binding)
            where TViewModel : class, IViewModel
            where TTag : struct, ITag
        {
            var key = new LinkKey(gid, typeof(TViewModel));
            if (!_linksByKey.TryGetValue(key, out var link))
                return;

            var typedEntry = (LinkEntry<TViewModel>)link;
            binding(typedEntry.Link.ViewModel, isPresent);
        }

        private void PollMulti<TViewModel, TElement>(EcsMultiBinding<TWorld, TViewModel, TElement> binding)
            where TViewModel : class, IViewModel
            where TElement : struct, IMultiComponent
        {
            if (!_linksByViewModelType.TryGetValue(typeof(TViewModel), out var links))
                return;

            for (var i = 0; i < links.Count; i++)
            {
                var link = links[i];
                var entity = link.EntityGID.Unpack<TWorld>();

                if (!entity.Has<World<TWorld>.Multi<TElement>>())
                    continue;

                ref readonly var multi = ref entity.Read<World<TWorld>.Multi<TElement>>();
                var typedEntry = (LinkEntry<TViewModel>)link;
                binding(typedEntry.Link.ViewModel, in multi);
            }
        }

        private interface ILink
        {
            EntityGID EntityGID { get; }
            Type ViewModelType { get; }
            bool IsDisposed { get; }
            void DisposeFromRegistry();
        }

        private sealed class LinkEntry<TViewModel> : ILink
            where TViewModel : class, IViewModel
        {
            public LinkEntry(EcsLink<TWorld, TViewModel> link)
            {
                Link = link;
            }

            public EcsLink<TWorld, TViewModel> Link { get; }

            public EntityGID EntityGID => Link.EntityGID;

            public Type ViewModelType => typeof(TViewModel);

            public bool IsDisposed => Link.IsDisposed;

            public void DisposeFromRegistry()
            {
                Link.DisposeFromRegistry();
            }
        }

        private interface IBinding
        {
            BindingKind Kind { get; }
            Type ViewModelType { get; }
            Type EcsType { get; }
            void ApplyInitial(ILink link, World<TWorld>.Entity entity);
            void Sync(EcsLinkRegistry<TWorld> registry);
        }

        private sealed class ComponentBinding<TViewModel, TComponent> : IBinding
            where TViewModel : class, IViewModel
            where TComponent : struct, IComponent, ITrackableAdded, ITrackableChanged
        {
            private readonly EcsComponentBinding<TViewModel, TComponent> _binding;

            public ComponentBinding(EcsComponentBinding<TViewModel, TComponent> binding)
            {
                _binding = binding;
            }

            public BindingKind Kind => BindingKind.Component;

            public Type ViewModelType => typeof(TViewModel);

            public Type EcsType => typeof(TComponent);

            public void ApplyInitial(ILink link, World<TWorld>.Entity entity)
            {
                if (!entity.Has<TComponent>())
                    return;

                ref readonly var component = ref entity.Read<TComponent>();
                var typedEntry = (LinkEntry<TViewModel>)link;
                _binding(typedEntry.Link.ViewModel, in component);
            }

            public void Sync(EcsLinkRegistry<TWorld> registry)
            {
                foreach (var entity in World<TWorld>.Query<All<TComponent>, AllAdded<TComponent>>().Entities())
                    Apply(registry, entity);

                foreach (var entity in World<TWorld>.Query<All<TComponent>, AllChanged<TComponent>, NoneAdded<TComponent>>().Entities())
                    Apply(registry, entity);
            }

            private void Apply(EcsLinkRegistry<TWorld> registry, World<TWorld>.Entity entity)
            {
                ref readonly var component = ref entity.Read<TComponent>();
                registry.ApplyComponent(entity.GID, in component, _binding);
            }
        }

        private sealed class TagBinding<TViewModel, TTag> : IBinding
            where TViewModel : class, IViewModel
            where TTag : struct, ITag, ITrackableAdded, ITrackableDeleted
        {
            private readonly EcsTagBinding<TViewModel, TTag> _binding;

            public TagBinding(EcsTagBinding<TViewModel, TTag> binding)
            {
                _binding = binding;
            }

            public BindingKind Kind => BindingKind.Tag;

            public Type ViewModelType => typeof(TViewModel);

            public Type EcsType => typeof(TTag);

            public void ApplyInitial(ILink link, World<TWorld>.Entity entity)
            {
                var typedEntry = (LinkEntry<TViewModel>)link;
                _binding(typedEntry.Link.ViewModel, entity.Has<TTag>());
            }

            public void Sync(EcsLinkRegistry<TWorld> registry)
            {
                foreach (var entity in World<TWorld>.Query<All<TTag>, AllAdded<TTag>>().Entities())
                    registry.ApplyTag(entity.GID, true, _binding);

                foreach (var entity in World<TWorld>.Query<AllDeleted<TTag>>().Entities())
                    registry.ApplyTag(entity.GID, false, _binding);
            }
        }

        private sealed class MultiBinding<TViewModel, TElement> : IBinding
            where TViewModel : class, IViewModel
            where TElement : struct, IMultiComponent
        {
            private readonly EcsMultiBinding<TWorld, TViewModel, TElement> _binding;

            public MultiBinding(EcsMultiBinding<TWorld, TViewModel, TElement> binding)
            {
                _binding = binding;
            }

            public BindingKind Kind => BindingKind.Multi;

            public Type ViewModelType => typeof(TViewModel);

            public Type EcsType => typeof(TElement);

            public void ApplyInitial(ILink link, World<TWorld>.Entity entity)
            {
                if (!entity.Has<World<TWorld>.Multi<TElement>>())
                    return;

                ref readonly var multi = ref entity.Read<World<TWorld>.Multi<TElement>>();
                var typedEntry = (LinkEntry<TViewModel>)link;
                _binding(typedEntry.Link.ViewModel, in multi);
            }

            public void Sync(EcsLinkRegistry<TWorld> registry)
            {
                registry.PollMulti(_binding);
            }
        }

        private enum BindingKind
        {
            Component = 0,
            Tag = 1,
            Multi = 2
        }

        private readonly struct LinkKey : IEquatable<LinkKey>
        {
            private readonly EntityGID _entityGID;
            private readonly Type _viewModelType;

            public LinkKey(EntityGID entityGID, Type viewModelType)
            {
                _entityGID = entityGID;
                _viewModelType = viewModelType;
            }

            public bool Equals(LinkKey other)
            {
                return _entityGID == other._entityGID && _viewModelType == other._viewModelType;
            }

            public override bool Equals(object obj)
            {
                return obj is LinkKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_entityGID.Raw, _viewModelType);
            }
        }

        private readonly struct BindingKey : IEquatable<BindingKey>
        {
            private readonly BindingKind _kind;
            private readonly Type _viewModelType;
            private readonly Type _ecsType;

            public BindingKey(BindingKind kind, Type viewModelType, Type ecsType)
            {
                _kind = kind;
                _viewModelType = viewModelType;
                _ecsType = ecsType;
            }

            public bool Equals(BindingKey other)
            {
                return _kind == other._kind
                       && _viewModelType == other._viewModelType
                       && _ecsType == other._ecsType;
            }

            public override bool Equals(object obj)
            {
                return obj is BindingKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine((int)_kind, _viewModelType, _ecsType);
            }
        }
    }
}
