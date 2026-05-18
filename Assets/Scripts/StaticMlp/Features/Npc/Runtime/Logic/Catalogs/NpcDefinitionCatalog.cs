using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Npc
{
    public static class NpcDefinitionCatalog
    {
        public static readonly NpcDefinitionId ExtractedCompanionId = new(1);
        public static readonly NpcDefinitionId RescuedSpecialistId = new(2);
        public static readonly NpcDefinitionId IncubatedResearcherId = new(3);
        public static readonly NpcDefinitionId SeededCampBuilderId = new(4);

        private static readonly NpcDefinition[] Definitions =
        {
            new(
                ExtractedCompanionId,
                NpcClass.Companion,
                NpcRoleFlags.Guard | NpcRoleFlags.Hauler,
                NpcAcquisitionPathFlags.Extraction),
            new(
                RescuedSpecialistId,
                NpcClass.Specialist,
                NpcRoleFlags.Researcher,
                NpcAcquisitionPathFlags.Rescue),
            new(
                IncubatedResearcherId,
                NpcClass.Specialist,
                NpcRoleFlags.Processor | NpcRoleFlags.Researcher,
                NpcAcquisitionPathFlags.Incubation),
            new(
                SeededCampBuilderId,
                NpcClass.Companion,
                NpcRoleFlags.Builder,
                NpcAcquisitionPathFlags.Seeded)
        };

        static NpcDefinitionCatalog()
        {
            NpcDefinitionCatalogValidator.Validate(Definitions);
        }

        public static IReadOnlyList<NpcDefinition> All => Definitions;

        public static NpcDefinition Get(NpcDefinitionId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(NpcDefinition)} for npc definition id {id.Value} in {nameof(NpcDefinitionCatalog)}.");
        }

        public static bool TryGet(NpcDefinitionId id, out NpcDefinition definition)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                definition = Definitions[i];
                return true;
            }

            definition = default;
            return false;
        }
    }
}
