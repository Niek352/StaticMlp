using System;
using NUnit.Framework;
using StaticMlp.Features.Npc;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcDefinitionCatalogTests
    {
        [Test]
        public void CurrentCatalog_Validates()
        {
            Assert.DoesNotThrow(() => NpcDefinitionCatalogValidator.Validate(NpcDefinitionCatalog.All));
        }

        [Test]
        public void Get_ExistingDefinition_ReturnsDefinition()
        {
            var definition = NpcDefinitionCatalog.Get(NpcDefinitionCatalog.ExtractedCompanionId);

            Assert.That(definition.Id, Is.EqualTo(NpcDefinitionCatalog.ExtractedCompanionId));
            Assert.That(definition.Class, Is.EqualTo(NpcClass.Companion));
            Assert.That(definition.AllowedAcquisitionPaths, Is.EqualTo(NpcAcquisitionPathFlags.Extraction));
        }

        [Test]
        public void SeededWorkerDefinitions_HaveExpectedRoleFlags()
        {
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededBuilderId, NpcClass.Companion, NpcRoleFlags.Builder);
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededCampBuilderId, NpcClass.Companion, NpcRoleFlags.Builder);
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededGathererId, NpcClass.Companion, NpcRoleFlags.Gatherer);
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededHaulerId, NpcClass.Companion, NpcRoleFlags.Hauler);
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededProcessorId, NpcClass.Specialist, NpcRoleFlags.Processor);
            AssertSeededWorkerDefinition(NpcDefinitionCatalog.SeededGuardId, NpcClass.Companion, NpcRoleFlags.Guard);
        }

        [Test]
        public void Validate_DuplicateId_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new NpcDefinitionId(1), NpcClass.Companion, NpcRoleFlags.Guard, NpcAcquisitionPathFlags.Extraction),
                CreateDefinition(new NpcDefinitionId(1), NpcClass.Specialist, NpcRoleFlags.Researcher, NpcAcquisitionPathFlags.Rescue)
            };

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingClass_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new NpcDefinitionId(1), NpcClass.None, NpcRoleFlags.Guard, NpcAcquisitionPathFlags.Extraction)
            };

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingAcquisitionPath_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new NpcDefinitionId(1), NpcClass.Companion, NpcRoleFlags.Guard, NpcAcquisitionPathFlags.None)
            };

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingRoles_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new NpcDefinitionId(1), NpcClass.Companion, NpcRoleFlags.None, NpcAcquisitionPathFlags.Extraction)
            };

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_SpecialistWithOnlyRawLaborRoles_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(
                    new NpcDefinitionId(1),
                    NpcClass.Specialist,
                    NpcRoleFlags.Gatherer | NpcRoleFlags.Hauler,
                    NpcAcquisitionPathFlags.Rescue)
            };

            Assert.Throws<InvalidOperationException>(() => NpcDefinitionCatalogValidator.Validate(definitions));
        }

        private static NpcDefinition CreateDefinition(
            NpcDefinitionId id,
            NpcClass npcClass,
            NpcRoleFlags roles,
            NpcAcquisitionPathFlags allowedAcquisitionPaths)
        {
            return new NpcDefinition(id, npcClass, roles, allowedAcquisitionPaths);
        }

        private static void AssertSeededWorkerDefinition(
            NpcDefinitionId definitionId,
            NpcClass npcClass,
            NpcRoleFlags roles)
        {
            var definition = NpcDefinitionCatalog.Get(definitionId);

            Assert.That(definition.Id, Is.EqualTo(definitionId));
            Assert.That(definition.Class, Is.EqualTo(npcClass));
            Assert.That(definition.Roles, Is.EqualTo(roles));
            Assert.That(definition.AllowedAcquisitionPaths, Is.EqualTo(NpcAcquisitionPathFlags.Seeded));
        }
    }
}
