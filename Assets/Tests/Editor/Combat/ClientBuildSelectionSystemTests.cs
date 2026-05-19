using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientLoadoutSelectionSystemTests
    {
        [Test]
        public void Update_WhenSelectionChangesWithoutCommit_RefreshesPreparedLoadoutSnapshotOnly()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            player.Set(new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientLoadoutSelectionSyncState
            {
                LastSentPrimaryModuleId = LoadoutModuleCatalog.PoisonArrowModuleId,
                ShouldCommitSelection = false
            });

            var system = new ClientLoadoutSelectionSystem();
            system.Update();

            Assert.That(player.Read<OwnerLoadoutSelection>().PrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));

            ref readonly var snapshot = ref player.Read<PreparedLoadoutSnapshot>();
            Assert.That(snapshot.ArchetypeId, Is.EqualTo(LoadoutArchetypeCatalog.FireBomberId));
            Assert.That(snapshot.PrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            Assert.That(snapshot.PreparedAbilityId, Is.EqualTo(CombatAbilityId.FireFlask));
            Assert.That(snapshot.FallbackAbilityId, Is.EqualTo(CombatAbilityId.BasicMeleeAuto));
            Assert.That(player.Read<ClientLoadoutSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.PoisonArrowModuleId));
        }

        [Test]
        public void Update_WhenCommitRequested_SendsPrepareLoadoutCommand()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            player.Set(new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientLoadoutSelectionSyncState
            {
                ShouldCommitSelection = true
            });

            var system = new ClientLoadoutSelectionSystem();
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();
            system.Update();
            sendSystem.Update();

            Assert.That(player.Read<ClientLoadoutSelectionSyncState>().ShouldCommitSelection, Is.False);
            Assert.That(player.Read<ClientLoadoutSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void Update_WhenBossBuildIsCommitted_DoesNotSendCommand()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            var anchor = CW.NewEntity<Default>();
            anchor.Set(new BossLoadoutPreparationState
            {
                Status = BossLoadoutPreparationStatus.Committed
            });

            player.Set(new OwnerLoadoutSelection
            {
                PrimaryModuleId = LoadoutModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientLoadoutSelectionSyncState
            {
                ShouldCommitSelection = true
            });

            var system = new ClientLoadoutSelectionSystem();
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();
            system.Update();
            sendSystem.Update();

            Assert.That(player.Read<ClientLoadoutSelectionSyncState>().LastSentPrimaryModuleId.Value, Is.EqualTo(0));
            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Is.Empty);
            sendSystem.Destroy();
        }
    }
}
