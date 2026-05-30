using System;
using Aspid.MVVM;
using Aspid.StaticEcs;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace Aspid.StaticEcs.Tests
{
    public sealed class EcsLinkRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            if (TestW.Status != WorldStatus.NotCreated)
                TestW.Destroy();

            TestW.Create(WorldConfig.Default());
            TestW.Types()
                .Component<TestComponent>()
                .Tag<TestTag>()
                .Component<World<TestWorld>.Multi<TestMultiElement>>();
            TestW.Initialize();
            TestW.SetResource(new EcsLinkRegistry<TestWorld>());
        }

        [TearDown]
        public void TearDown()
        {
            if (TestW.Status != WorldStatus.NotCreated)
                TestW.Destroy();
        }

        [Test]
        public void Create_AppliesInitialComponentTagAndMultiState()
        {
            var entity = TestW.NewEntity<Default>();
            entity.Set(new TestComponent { Value = 7 });
            entity.Set<TestTag>();
            ref var multi = ref entity.Add<World<TestWorld>.Multi<TestMultiElement>>();
            multi.Add(new TestMultiElement { Value = 3 });
            multi.Add(new TestMultiElement { Value = 4 });

            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            registry.RegisterComponent<TestViewModel, TestComponent>(ApplyTestComponent);
            registry.RegisterTag<TestViewModel, TestTag>(ApplyTestTag);
            registry.RegisterMulti<TestViewModel, TestMultiElement>(ApplyTestMulti);

            var link = registry.Create(entity, static _ => new TestViewModel());

            Assert.That(link.EntityGID.Raw, Is.EqualTo(entity.GID.Raw));
            Assert.That(link.ViewModel.ComponentValue, Is.EqualTo(7));
            Assert.That(link.ViewModel.ComponentApplyCount, Is.EqualTo(1));
            Assert.That(link.ViewModel.TagValue, Is.True);
            Assert.That(link.ViewModel.TagApplyCount, Is.EqualTo(1));
            Assert.That(link.ViewModel.MultiSum, Is.EqualTo(7));
            Assert.That(link.ViewModel.MultiLength, Is.EqualTo(2));
            Assert.That(link.ViewModel.MultiApplyCount, Is.EqualTo(1));
        }

        [Test]
        public void Sync_AppliesAddedAndChangedComponents()
        {
            var entity = TestW.NewEntity<Default>();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            registry.RegisterComponent<TestViewModel, TestComponent>(ApplyTestComponent);
            var link = registry.Create(entity, static _ => new TestViewModel());

            TestW.Tick();
            entity.Set(new TestComponent { Value = 5 });
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.ComponentValue, Is.EqualTo(5));
            Assert.That(link.ViewModel.ComponentApplyCount, Is.EqualTo(1));

            TestW.Tick();
            ref var component = ref entity.Mut<TestComponent>();
            component.Value = 9;
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.ComponentValue, Is.EqualTo(9));
            Assert.That(link.ViewModel.ComponentApplyCount, Is.EqualTo(2));
        }

        [Test]
        public void Sync_AppliesAddedAndDeletedTags()
        {
            var entity = TestW.NewEntity<Default>();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            registry.RegisterTag<TestViewModel, TestTag>(ApplyTestTag);
            var link = registry.Create(entity, static _ => new TestViewModel());

            Assert.That(link.ViewModel.TagValue, Is.False);
            Assert.That(link.ViewModel.TagApplyCount, Is.EqualTo(1));

            TestW.Tick();
            entity.Set<TestTag>();
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.TagValue, Is.True);
            Assert.That(link.ViewModel.TagApplyCount, Is.EqualTo(2));

            TestW.Tick();
            entity.Delete<TestTag>();
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.TagValue, Is.False);
            Assert.That(link.ViewModel.TagApplyCount, Is.EqualTo(3));
        }

        [Test]
        public void Sync_PollsMultiComponents()
        {
            var entity = TestW.NewEntity<Default>();
            ref var initialMulti = ref entity.Add<World<TestWorld>.Multi<TestMultiElement>>();
            initialMulti.Add(new TestMultiElement { Value = 2 });

            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            registry.RegisterMulti<TestViewModel, TestMultiElement>(ApplyTestMulti);
            var link = registry.Create(entity, static _ => new TestViewModel());

            Assert.That(link.ViewModel.MultiSum, Is.EqualTo(2));
            Assert.That(link.ViewModel.MultiLength, Is.EqualTo(1));
            Assert.That(link.ViewModel.MultiApplyCount, Is.EqualTo(1));

            TestW.Tick();
            ref var grownMulti = ref entity.Ref<World<TestWorld>.Multi<TestMultiElement>>();
            grownMulti.Add(new TestMultiElement { Value = 5 });
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.MultiSum, Is.EqualTo(7));
            Assert.That(link.ViewModel.MultiLength, Is.EqualTo(2));
            Assert.That(link.ViewModel.MultiApplyCount, Is.EqualTo(2));

            TestW.Tick();
            ref var reducedMulti = ref entity.Ref<World<TestWorld>.Multi<TestMultiElement>>();
            reducedMulti.RemoveAt(0);
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(link.ViewModel.MultiSum, Is.EqualTo(5));
            Assert.That(link.ViewModel.MultiLength, Is.EqualTo(1));
            Assert.That(link.ViewModel.MultiApplyCount, Is.EqualTo(3));
        }

        [Test]
        public void Create_ThrowsForDuplicateEntityViewModelLink()
        {
            var entity = TestW.NewEntity<Default>();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();

            registry.Create(entity, static _ => new TestViewModel());

            Assert.Throws<InvalidOperationException>(() =>
            {
                registry.Create(entity, static _ => new TestViewModel());
            });
        }

        [Test]
        public void Attach_AppliesInitialStateWithoutOwningViewModelByDefault()
        {
            var entity = TestW.NewEntity<Default>();
            entity.Set(new TestComponent { Value = 13 });
            var viewModel = new TestViewModel();

            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            registry.RegisterComponent<TestViewModel, TestComponent>(ApplyTestComponent);
            var link = registry.Attach(entity, viewModel);

            Assert.That(link.ViewModel, Is.SameAs(viewModel));
            Assert.That(viewModel.ComponentValue, Is.EqualTo(13));
            Assert.That(viewModel.ComponentApplyCount, Is.EqualTo(1));

            link.Dispose();

            Assert.That(viewModel.IsDisposed, Is.False);
            Assert.DoesNotThrow(() =>
            {
                registry.Attach(entity, new TestViewModel());
            });
        }

        [Test]
        public void Attach_DisposesViewModelWhenRequested()
        {
            var entity = TestW.NewEntity<Default>();
            var viewModel = new TestViewModel();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();

            var link = registry.Attach(entity, viewModel, disposeViewModel: true);
            link.Dispose();

            Assert.That(viewModel.IsDisposed, Is.True);
        }

        [Test]
        public void Sync_DisposesViewModelWhenLinkedEntityDies()
        {
            var entity = TestW.NewEntity<Default>();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            var viewModel = new TestViewModel();
            registry.Create(entity, _ => viewModel);

            entity.Destroy();
            new EcsLinkSyncSystem<TestWorld>().Update();

            Assert.That(viewModel.IsDisposed, Is.True);
        }

        [Test]
        public void Dispose_RemovesLinkAndDisposesViewModel()
        {
            var entity = TestW.NewEntity<Default>();
            var registry = TestW.GetResource<EcsLinkRegistry<TestWorld>>();
            var link = registry.Create(entity, static _ => new TestViewModel());

            link.Dispose();

            Assert.That(link.ViewModel.IsDisposed, Is.True);
            Assert.DoesNotThrow(() =>
            {
                registry.Create(entity, static _ => new TestViewModel());
            });
        }

        private struct TestWorld : IWorldType
        {
        }

        private static void ApplyTestComponent(TestViewModel viewModel, in TestComponent component)
        {
            viewModel.ApplyComponent(in component);
        }

        private static void ApplyTestTag(TestViewModel viewModel, bool isPresent)
        {
            viewModel.ApplyTag(isPresent);
        }

        private static void ApplyTestMulti(TestViewModel viewModel, in World<TestWorld>.Multi<TestMultiElement> multi)
        {
            viewModel.ApplyMulti(in multi);
        }

        private abstract class TestW : World<TestWorld>
        {
        }

        private struct TestComponent : IComponent, ITrackableAdded, ITrackableChanged
        {
            public int Value;
        }

        private struct TestTag : ITag, ITrackableAdded, ITrackableDeleted
        {
        }

        private struct TestMultiElement : IMultiComponent
        {
            public int Value;
        }

        private sealed class TestViewModel : IViewModel, IDisposable
        {
            public int ComponentValue;
            public int ComponentApplyCount;
            public bool TagValue;
            public int TagApplyCount;
            public int MultiSum;
            public int MultiLength;
            public int MultiApplyCount;
            public bool IsDisposed;

            public FindBindableMemberResult FindBindableMember(in FindBindableMemberParameters parameters)
            {
                return default;
            }

            public void ApplyComponent(in TestComponent component)
            {
                ComponentValue = component.Value;
                ComponentApplyCount++;
            }

            public void ApplyTag(bool isPresent)
            {
                TagValue = isPresent;
                TagApplyCount++;
            }

            public void ApplyMulti(in World<TestWorld>.Multi<TestMultiElement> multi)
            {
                var sum = 0;

                for (var i = 0; i < multi.Length; i++)
                {
                    ref readonly var item = ref multi.Get(i);
                    sum += item.Value;
                }

                MultiSum = sum;
                MultiLength = multi.Length;
                MultiApplyCount++;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
