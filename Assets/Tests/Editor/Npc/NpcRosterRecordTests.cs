using System;
using NUnit.Framework;
using StaticMlp.Features.Npc;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcRosterRecordTests
    {
        [Test]
        public void FactoryCreatesRosterRecord_WithCorrectFields()
        {
            var definition = NpcDefinitionCatalog.Get(NpcDefinitionCatalog.ExtractedCompanionId);
            var tick = 42u;

            var spec = new NpcRosterRecordSpawnSpec(
                definition.Id.Value,
                definition.Class,
                NpcAcquisitionPath.Extraction,
                NpcRosterState.Captured,
                tick);

            Assert.That(spec.DefinitionId, Is.EqualTo(definition.Id.Value));
            Assert.That(spec.Class, Is.EqualTo(definition.Class));
            Assert.That(spec.AcquisitionPath, Is.EqualTo(NpcAcquisitionPath.Extraction));
            Assert.That(spec.State, Is.EqualTo(NpcRosterState.Captured));
            Assert.That(spec.CreatedServerTick, Is.EqualTo(tick));
        }

        [Test]
        public void CatalogLookup_InvalidDefinitionId_Throws()
        {
            var invalidId = new NpcDefinitionId(999);

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalog.Get(invalidId));
        }

        [Test]
        public void NpcRosterRecord_UsesValueTypeSemantics()
        {
            var record = new NpcRosterRecord
            {
                DefinitionId = 1,
                Class = NpcClass.Companion,
                AcquisitionPath = NpcAcquisitionPath.Rescue,
                State = NpcRosterState.Recruited,
                CreatedServerTick = 100u
            };

            Assert.That(record.Definition, Is.EqualTo(new NpcDefinitionId(1)));
            Assert.That(record.Class, Is.EqualTo(NpcClass.Companion));
            Assert.That(record.AcquisitionPath, Is.EqualTo(NpcAcquisitionPath.Rescue));
            Assert.That(record.State, Is.EqualTo(NpcRosterState.Recruited));
            Assert.That(record.CreatedServerTick, Is.EqualTo(100u));
        }
    }
}
