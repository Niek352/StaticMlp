using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class BedrollShelterOperationTests
    {
        [Test]
        public void BedSlotRules_ReserveOccupyAndRelease()
        {
            var worker = new EntityGID(10);
            var reserved = BedSlotRules.Reserve(new BedSlotState(0, BedSlotStatus.Free), worker);
            var occupied = BedSlotRules.Occupy(reserved, worker);
            var released = BedSlotRules.Release(occupied);

            Assert.That(reserved.Status, Is.EqualTo(BedSlotStatus.Reserved));
            Assert.That(reserved.Occupant, Is.EqualTo(worker));
            Assert.That(occupied.Status, Is.EqualTo(BedSlotStatus.Occupied));
            Assert.That(released.Status, Is.EqualTo(BedSlotStatus.Free));
            Assert.That(released.Occupant, Is.EqualTo(default(EntityGID)));
        }

        [Test]
        public void BedSlotRules_BlockedSlotRejectsReservation()
        {
            Assert.Throws<InvalidOperationException>(() =>
                BedSlotRules.Reserve(new BedSlotState(0, BedSlotStatus.Blocked), new EntityGID(10)));
        }

        [Test]
        public void ServerBedrollShelterBootstrapSystem_AddsShelterAndSlots()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var finished = scope.CreateFinishedBuilding();
            var system = new ServerBedrollShelterBootstrapSystem();
            system.Init();

            SW.SendEvent(new BuildingConstructionCompletedEvent(
                finished.GID,
                BuildingCatalogData.BedrollShelterId,
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Quaternion.identity));
            system.Update();
            system.Destroy();

            Assert.That(finished.Read<BedrollShelterState>().Enabled, Is.True);
            Assert.That(finished.Read<BedrollShelterState>().SlotCount, Is.EqualTo(2));

            ref var slots = ref finished.Ref<SW.Multi<BedSlotState>>();
            Assert.That(slots.Length, Is.EqualTo(2));
            Assert.That(slots[0].Status, Is.EqualTo(BedSlotStatus.Free));
            Assert.That(slots[1].Status, Is.EqualTo(BedSlotStatus.Free));
        }
    }
}
