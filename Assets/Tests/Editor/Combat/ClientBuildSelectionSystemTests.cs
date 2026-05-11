using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientBuildSelectionSystemTests
    {
        [Test]
        public void Update_WhenSelectionChangesWithoutCommit_RefreshesPreparedBuildSnapshotOnly()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientBuildSelectionSyncState
            {
                LastSentPrimaryModuleId = BuildModuleCatalog.PoisonArrowModuleId,
                ShouldCommitSelection = false
            });

            var system = new ClientBuildSelectionSystem();
            system.Update();

            Assert.That(player.Read<OwnerBuildSelection>().PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));

            ref readonly var snapshot = ref player.Read<PreparedBuildSnapshot>();
            Assert.That(snapshot.ArchetypeId, Is.EqualTo(BuildArchetypeCatalog.FireBomberId));
            Assert.That(snapshot.PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(snapshot.PreparedAbilityId, Is.EqualTo(CombatAbilityId.FireFlask));
            Assert.That(snapshot.FallbackAbilityId, Is.EqualTo(CombatAbilityId.BasicMeleeAuto));
            Assert.That(player.Read<ClientBuildSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.PoisonArrowModuleId));
        }

        [Test]
        public void Update_WhenCommitRequested_SendsPrepareBuildCommand()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientBuildSelectionSyncState
            {
                ShouldCommitSelection = true
            });

            var system = new ClientBuildSelectionSystem();
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();
            system.Update();
            sendSystem.Update();

            Assert.That(player.Read<ClientBuildSelectionSyncState>().ShouldCommitSelection, Is.False);
            Assert.That(player.Read<ClientBuildSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(CW.GetResource<NetOutbox>().NetworkEventPackets, Has.Count.EqualTo(1));
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
            anchor.Set(new BossBuildPreparationState
            {
                Status = BossBuildPreparationStatus.Committed
            });

            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(new ClientBuildSelectionSyncState
            {
                ShouldCommitSelection = true
            });

            var system = new ClientBuildSelectionSystem();
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();
            system.Update();
            sendSystem.Update();

            Assert.That(player.Read<ClientBuildSelectionSyncState>().LastSentPrimaryModuleId.Value, Is.EqualTo(0));
            Assert.That(CW.GetResource<NetOutbox>().NetworkEventPackets, Is.Empty);
            sendSystem.Destroy();
        }
    }
}
