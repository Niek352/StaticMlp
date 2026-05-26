using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Tests.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudPresentationTests
    {
        [Test]
        public void Build_NoLocalInventory_ReturnsHiddenPresentation()
        {
            using var scope = new ClientInventoryHudWorldScope();
            scope.CreateLocalPlayerWithoutInventory();

            var presentation = ResourcesInventoryHudPresentationBuilder.Build();

            Assert.That(presentation.IsReady, Is.False);
            Assert.That(presentation.UsedSlots, Is.EqualTo(0));
            Assert.That(presentation.Capacity, Is.EqualTo(ResourcesInventory.MAX_SLOTS));
            Assert.That(presentation.Slots.Length, Is.EqualTo(0));
        }

        [Test]
        public void Build_LocalInventory_FillsFixedSlotsInRowOrder()
        {
            using var scope = new ClientInventoryHudWorldScope();
            scope.CreateInventoryPlayer(
                new CarriedResourceEntry(ResourceCatalog.WoodId, ResourcesInventory.MAX_STACK_AMOUNT),
                new CarriedResourceEntry(ResourceCatalog.WoodId, 3),
                new CarriedResourceEntry(ResourceCatalog.StoneId, 4));

            var presentation = ResourcesInventoryHudPresentationBuilder.Build();

            Assert.That(presentation.IsReady, Is.True);
            Assert.That(presentation.Slots.Length, Is.EqualTo(ResourcesInventory.MAX_SLOTS));
            Assert.That(presentation.Slots[0].IsOccupied, Is.True);
            Assert.That(presentation.Slots[0].ResourceName, Is.EqualTo("Wood"));
            Assert.That(presentation.Slots[0].Amount, Is.EqualTo(ResourcesInventory.MAX_STACK_AMOUNT));
            Assert.That(presentation.Slots[1].IsOccupied, Is.True);
            Assert.That(presentation.Slots[1].ResourceName, Is.EqualTo("Wood"));
            Assert.That(presentation.Slots[1].Amount, Is.EqualTo(3));
            Assert.That(presentation.Slots[2].IsOccupied, Is.True);
            Assert.That(presentation.Slots[2].ResourceName, Is.EqualTo("Stone"));
            Assert.That(presentation.Slots[2].Amount, Is.EqualTo(4));

            for (var i = 3; i < presentation.Slots.Length; i++)
            {
                Assert.That(presentation.Slots[i].IsOccupied, Is.False);
                Assert.That(presentation.Slots[i].ResourceName, Is.EqualTo(string.Empty));
                Assert.That(presentation.Slots[i].Amount, Is.EqualTo(0));
            }
        }

        [Test]
        public void Build_LocalInventory_ReportsUsedSlotsAndCapacity()
        {
            using var scope = new ClientInventoryHudWorldScope();
            scope.CreateInventoryPlayer(
                new CarriedResourceEntry(ResourceCatalog.WoodId, 6),
                new CarriedResourceEntry(ResourceCatalog.StoneId, 2));

            var presentation = ResourcesInventoryHudPresentationBuilder.Build();

            Assert.That(presentation.UsedSlots, Is.EqualTo(2));
            Assert.That(presentation.Capacity, Is.EqualTo(ResourcesInventory.MAX_SLOTS));
        }

        [Test]
        public void Build_MultipleLocalInventories_Throws()
        {
            using var scope = new ClientInventoryHudWorldScope();
            scope.CreateInventoryPlayer(new CarriedResourceEntry(ResourceCatalog.WoodId, 1));
            scope.CreateInventoryPlayer(new CarriedResourceEntry(ResourceCatalog.StoneId, 1));

            Assert.Throws<InvalidOperationException>(() => ResourcesInventoryHudPresentationBuilder.Build());
        }

        private sealed class ClientInventoryHudWorldScope : IDisposable
        {
            public ClientInventoryHudWorldScope()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ClientCoreWT).Assembly,
                    typeof(LocalOwned).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(ResourcesInventory).Assembly);
                CW.Initialize();
            }

            public CW.Entity CreateLocalPlayerWithoutInventory()
            {
                var player = CW.NewEntity<Default>();
                player.Set<LocalOwned>();
                player.Set<PlayerTag>();
                return player;
            }

            public CW.Entity CreateInventoryPlayer(params CarriedResourceEntry[] rows)
            {
                var player = CreateLocalPlayerWithoutInventory();
                player.Set(new ResourcesInventory
                {
                    Capacity = ResourcesInventory.MAX_SLOTS
                });

                ref var inventoryRows = ref player.Add<CW.Multi<CarriedResourceEntry>>();
                for (var i = 0; i < rows.Length; i++)
                    inventoryRows.Add(rows[i]);

                return player;
            }

            public void Dispose()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }
    }
}
