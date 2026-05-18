using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Npc
{
    public static class NpcDefinitionCatalogValidator
    {
        private const NpcAcquisitionPathFlags ALL_ACQUISITION_PATHS =
            NpcAcquisitionPathFlags.Extraction
            | NpcAcquisitionPathFlags.Rescue
            | NpcAcquisitionPathFlags.Incubation
            | NpcAcquisitionPathFlags.Seeded;

        private const NpcRoleFlags RAW_LABOR_ROLES =
            NpcRoleFlags.Gatherer
            | NpcRoleFlags.Hauler
            | NpcRoleFlags.Builder;

        private const NpcRoleFlags ALL_ROLES =
            NpcRoleFlags.Gatherer
            | NpcRoleFlags.Hauler
            | NpcRoleFlags.Processor
            | NpcRoleFlags.Guard
            | NpcRoleFlags.Builder
            | NpcRoleFlags.Researcher;

        public static void Validate(IReadOnlyList<NpcDefinition> definitions)
        {
            var ids = new HashSet<NpcDefinitionId>();

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate npc definition id {definition.Id.Value}.");

                ValidateClass(definition);
                ValidateAcquisitionPaths(definition);
                ValidateRoles(definition);
            }
        }

        private static void ValidateClass(in NpcDefinition definition)
        {
            if (definition.Class != NpcClass.Companion && definition.Class != NpcClass.Specialist)
                throw new InvalidOperationException($"Npc definition id {definition.Id.Value} has invalid npc class {definition.Class}.");
        }

        private static void ValidateAcquisitionPaths(in NpcDefinition definition)
        {
            if (definition.AllowedAcquisitionPaths == NpcAcquisitionPathFlags.None)
                throw new InvalidOperationException($"Npc definition id {definition.Id.Value} has no allowed acquisition path.");

            if ((definition.AllowedAcquisitionPaths & ~ALL_ACQUISITION_PATHS) != 0)
            {
                throw new InvalidOperationException(
                    $"Npc definition id {definition.Id.Value} has unsupported acquisition path flags {definition.AllowedAcquisitionPaths}.");
            }
        }

        private static void ValidateRoles(in NpcDefinition definition)
        {
            if (definition.Roles == NpcRoleFlags.None)
                throw new InvalidOperationException($"Npc definition id {definition.Id.Value} has no npc roles.");

            if ((definition.Roles & ~ALL_ROLES) != 0)
                throw new InvalidOperationException($"Npc definition id {definition.Id.Value} has unsupported npc roles {definition.Roles}.");

            if (definition.Class == NpcClass.Specialist && (definition.Roles & ~RAW_LABOR_ROLES) == 0)
            {
                throw new InvalidOperationException(
                    $"Specialist npc definition id {definition.Id.Value} only has raw labor roles {definition.Roles}.");
            }
        }
    }
}
