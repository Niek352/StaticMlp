using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class BuildingEntityFactoryTests
    {
        [Test]
        public void SpawnConstructionSite_InitializesManifestAndSeedDerivedState()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(7);
            var definition = BuildingCatalogData.Get(BuildingCatalogData.CampCoreId);
            var gid = SW.GetResource<BuildingEntityFactory>().SpawnConstructionSite(new ConstructionSiteSpawnSpec(
                owner,
                definition,
                SettlementAnchorCatalog.HomeCampId,
                new Vector3(3f, 0f, 5f),
                Quaternion.identity,
                startReadyToBuild: true,
                initialBuildWork: 25f));

            Assert.That(gid.TryUnpack<ServerWT>(out var site), Is.True);
            Assert.That(site.Read<NetworkIdentity>().Owner, Is.EqualTo(owner));
            Assert.That(site.Read<SettlementAnchorRef>().Anchor, Is.EqualTo(SettlementAnchorCatalog.HomeCampId));
            Assert.That(site.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.BuildingInProgress));
            Assert.That(ConstructionResourcesAccess.GetDelivered(site, ResourceCatalog.WoodId), Is.EqualTo(10));
            Assert.That(ConstructionResourcesAccess.GetDelivered(site, ResourceCatalog.StoneId), Is.EqualTo(4));
            Assert.That(site.Read<ConstructionProgress>().BuildWorkDone, Is.EqualTo(25f));
        }

        [Test]
        public void ServerPlaceBuildingRequestSystem_SpawnsConstructionSiteBoundToHomeCampAnchor()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(3);
            scope.CreatePlayer(owner, Vector3.zero);

            var system = new ServerPlaceBuildingRequestSystem();
            system.Init();
            var request = new PlaceBuildingRequestEvent(
                BuildingCatalogData.CampCoreId.Value,
                new Vector3(20f, 0f, 20f),
                Quaternion.identity);
            SW.SendEvent(new NetworkEventFromClient<PlaceBuildingRequestEvent>(owner, in request));

            system.Update();
            system.Destroy();

            var spawnedCount = 0;
            foreach (var site in SW.Query<All<ConstructionSiteTag, SettlementAnchorRef, NetworkIdentity>>().Entities())
            {
                spawnedCount++;
                Assert.That(site.Read<NetworkIdentity>().Owner, Is.EqualTo(owner));
                Assert.That(site.Read<SettlementAnchorRef>().Anchor, Is.EqualTo(SettlementAnchorCatalog.HomeCampId));
            }

            Assert.That(spawnedCount, Is.EqualTo(1));
        }
    }
}
