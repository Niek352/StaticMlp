using System;
using NUnit.Framework;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Npc;
using StaticMlp.Game;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcRescueTests
    {
        private NpcTestServerWorldScope _scope;

        [SetUp]
        public void SetUp()
        {
            _scope = new NpcTestServerWorldScope();
        }

        [TearDown]
        public void TearDown()
        {
            _scope?.Dispose();
        }

        [Test]
        public void Rescue_LockedSite_IsRejected()
        {
            var site = CreateRescueSite(NpcRescueSiteState.Locked);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Rescue_ResolvedSite_IsRejected()
        {
            var site = CreateRescueSite(NpcRescueSiteState.Resolved);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Rescue_MissingDefinition_IsRejected()
        {
            var site = CreateRescueSite(NpcRescueSiteState.Rescuable, definitionId: 999);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Rescue_InvalidDefinitionForRescue_IsRejected()
        {
            var site = CreateRescueSite(
                NpcRescueSiteState.Rescuable,
                definitionId: NpcDefinitionCatalog.ExtractedCompanionId.Value);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Rescue_ValidSite_CreatesRosterRecord()
        {
            var site = CreateRescueSite(
                NpcRescueSiteState.Rescuable,
                definitionId: NpcDefinitionCatalog.RescuedSpecialistId.Value);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Accepted));
            Assert.That(result.RosterRecord, Is.Not.EqualTo(default(EntityGID)));
        }

        [Test]
        public void Rescue_ValidSite_BecomesResolved()
        {
            var site = CreateRescueSite(
                NpcRescueSiteState.Rescuable,
                definitionId: NpcDefinitionCatalog.RescuedSpecialistId.Value);
            var handler = new RescueNpcHandler();
            var request = new RescueNpcRequestEvent(site.GID);

            handler.Handle(new NetworkPeerId(1), request);

            Assert.That(site.Read<NpcRescueSite>().State, Is.EqualTo(NpcRescueSiteState.Resolved));
        }

        private SW.Entity CreateRescueSite(NpcRescueSiteState state, ushort definitionId = 0)
        {
            if (definitionId == 0)
                definitionId = NpcDefinitionCatalog.RescuedSpecialistId.Value;

            var entity = SW.NewEntity<Default>();
            entity.Set(new NpcRescueSite
            {
                NpcDefinitionId = definitionId,
                State = state
            });
            return entity;
        }
    }
}
