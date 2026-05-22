using System.Reflection;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Combat
{
    public sealed class ControllerResourceBridgeSystemTests
    {
        [SetUp]
        public void SetUp()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(SettlementPresentationFeature).Assembly);
            CW.Initialize();
            CW.SetResource(new SettlementHudState());
        }

        [TearDown]
        public void TearDown()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();
        }

        [Test]
        public void Update_WhenUnboundOrInactive_DoesNotApplyState()
        {
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            bridge.Update();

            var controller = new TestController(ControllerState.ViewFocused);
            bridge.Bind(controller);
            CW.SetResource(CreateHudState(10));

            bridge.Update();

            Assert.That(controller.ApplyCount, Is.EqualTo(0));
        }

        [Test]
        public void SyncOnce_AppliesCurrentResource()
        {
            var controller = new TestController(ControllerState.ViewFocused);
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            bridge.Bind(controller);
            CW.SetResource(CreateHudState(12));

            bridge.SyncOnce();

            Assert.That(controller.ApplyCount, Is.EqualTo(1));
            Assert.That(controller.LastValue, Is.EqualTo(12));
        }

        [Test]
        public void Update_WhenActivated_AppliesLatestResource()
        {
            var controller = new TestController(ControllerState.ViewFocused);
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            bridge.Bind(controller);
            bridge.Activate();

            CW.SetResource(CreateHudState(3));
            bridge.Update();
            CW.SetResource(CreateHudState(4));
            bridge.Update();

            Assert.That(controller.ApplyCount, Is.EqualTo(2));
            Assert.That(controller.LastValue, Is.EqualTo(4));
        }

        [Test]
        public void Update_WhenDeactivated_StopsApplyingState()
        {
            var controller = new TestController(ControllerState.ViewFocused);
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            bridge.Bind(controller);
            bridge.Activate();

            CW.SetResource(CreateHudState(5));
            bridge.Update();
            bridge.Deactivate();
            CW.SetResource(CreateHudState(6));
            bridge.Update();

            Assert.That(controller.ApplyCount, Is.EqualTo(1));
            Assert.That(controller.LastValue, Is.EqualTo(5));
        }

        [Test]
        public void Update_WhenControllerIsBlurredButVisible_AppliesState()
        {
            var controller = new TestController(ControllerState.ViewBlurred);
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            var binding = new BridgeSystemBinding<ControllerResourceBridgeSystem<TestController, SettlementHudState>, TestController>(controller, bridge);
            ((IMvcControllerModule)binding).OnBlur();

            CW.SetResource(CreateHudState(9));
            bridge.Update();

            Assert.That(controller.ApplyCount, Is.EqualTo(2));
            Assert.That(controller.LastValue, Is.EqualTo(9));
        }

        [Test]
        public void Binding_OnFocusAndViewShow_RefreshesImmediately()
        {
            var controller = new TestController(ControllerState.ViewBlurred);
            var bridge = new ControllerResourceBridgeSystem<TestController, SettlementHudState>();
            var binding = new BridgeSystemBinding<ControllerResourceBridgeSystem<TestController, SettlementHudState>, TestController>(controller, bridge);

            CW.SetResource(CreateHudState(7));
            ((IMvcControllerModule)binding).OnFocus();
            CW.SetResource(CreateHudState(8));
            ((IMvcControllerModule)binding).OnViewShow();

            Assert.That(controller.ApplyCount, Is.EqualTo(3));
            Assert.That(controller.LastValue, Is.EqualTo(8));
        }

        private static SettlementHudState CreateHudState(int wood)
        {
            var state = new SettlementHudState();
            state.Resources.Add(new SettlementResourceViewEntry(ResourceCatalog.WoodId, wood));
            return state;
        }

        private sealed class TestController : ControllerBase<Stage1HudView>, IResourcePresentationController<SettlementHudState>
        {
            private static readonly MethodInfo StateSetter = typeof(ControllerBase<Stage1HudView, ControllerNoData>)
                .GetProperty(nameof(State), BindingFlags.Instance | BindingFlags.Public)
                .GetSetMethod(true);

            public TestController(ControllerState state)
                : base(() => null)
            {
                StateSetter.Invoke(this, new object[] { state });
            }

            public int ApplyCount { get; private set; }
            public int LastValue { get; private set; }
            public override ViewLayer Layer => ViewLayer.Persistent;
            public override int? PersistentSortOrder => 0;

            public void Apply(in SettlementHudState state)
            {
                ApplyCount++;
                LastValue = state.Resources[0].Amount;
            }
        }
    }
}
