using FFS.Libraries.StaticEcs;

namespace Aspid.StaticEcs.Examples
{
    public static class EcsLinkExample
    {
        public static EcsLink<ExampleWorld, ExampleUnitViewModel> CreateLinkedUnit()
        {
            ExampleW.Create(WorldConfig.Default());
            ExampleW.Types()
                .Component<ExampleUnitHealth>()
                .Tag<ExampleUnitSelected>()
                .Component<World<ExampleWorld>.Multi<ExampleInventoryItem>>();
            ExampleW.Initialize();
            ExampleW.SetResource(new EcsLinkRegistry<ExampleWorld>());

            var registry = ExampleW.GetResource<EcsLinkRegistry<ExampleWorld>>();
            registry.RegisterComponent(static (ExampleUnitViewModel viewModel, in ExampleUnitHealth health) => viewModel.Apply(health));
            registry.RegisterTag<ExampleUnitViewModel, ExampleUnitSelected>(static (viewModel, isPresent) => viewModel.ApplySelected(isPresent));
            registry.RegisterMulti(static (ExampleUnitViewModel viewModel, in World<ExampleWorld>.Multi<ExampleInventoryItem> inventory) => viewModel.ApplyInventory(in inventory));

            var entity = ExampleW.NewEntity<Default>();
            entity.Set(new ExampleUnitHealth
            {
                Current = 10,
                Max = 20
            });

            ref var inventory = ref entity.Add<World<ExampleWorld>.Multi<ExampleInventoryItem>>();
            inventory.Add(new ExampleInventoryItem
            {
                Id = 1,
                Count = 3
            });

            return registry.Create(entity, static gid => new ExampleUnitViewModel(gid));
        }

        public static void ApplySampleChanges(EcsLink<ExampleWorld, ExampleUnitViewModel> link)
        {
            ExampleW.Tick();

            var entity = link.EntityGID.Unpack<ExampleWorld>();
            ref var health = ref entity.Mut<ExampleUnitHealth>();
            health.Current = 7;

            entity.Set<ExampleUnitSelected>();

            ref var inventory = ref entity.Ref<World<ExampleWorld>.Multi<ExampleInventoryItem>>();
            inventory.Add(new ExampleInventoryItem
            {
                Id = 2,
                Count = 5
            });

            new EcsLinkSyncSystem<ExampleWorld>().Update();
            ExampleW.Tick();
        }

        public static void DisposeLinkedUnit(EcsLink<ExampleWorld, ExampleUnitViewModel> link)
        {
            link.Dispose();
            ExampleW.Destroy();
        }
    }
}
