using System;

namespace StaticMlp.Features.CampFlow
{
    public static class CampFlowCatalog
    {
        public const string REPAIR_CAMP_OBJECTIVE = "Repair the camp core";
        public const string ASSIGN_WORKER_OBJECTIVE = "Assign the camp builder";
        public const string PLACE_STOCKPILE_OBJECTIVE = "Place a stockpile";
        public const string PLACE_SHELTER_OBJECTIVE = "Place worker shelter";
        public const string BRING_EXTRACTION_ONLINE_OBJECTIVE = "Bring extraction online";
        public const string BRING_WORKBENCH_ONLINE_OBJECTIVE = "Bring the workbench online";
        public const string PREPARE_BUILD_OBJECTIVE = "Choose the next combat build";
        public const string START_EXPEDITION_OBJECTIVE = "Start the nearby expedition";
        public const string CLEAR_EXPEDITION_OBJECTIVE = "Clear the hostile expedition";
        public const string DEFEND_CAMP_OBJECTIVE = "Defend the camp from the raid";
        public const string PREPARE_BOSS_OBJECTIVE = "Spend the reward on boss preparation";
        public const string START_BOSS_OBJECTIVE = "Begin the boss encounter";
        public const string DEFEAT_BOSS_OBJECTIVE = "Defeat the boss";
        public const string COMPLETE_OBJECTIVE = "Vertical slice complete";

        public const string GATHER_REPAIR_RESOURCES_HINT = "Gather the camp resources needed to begin repairs.";
        public const string CONTINUE_REPAIR_BUILD_HINT = "Resources delivered. Keep building the camp core to finish repairs.";
        public const string ASSIGN_WORKER_HINT = "The camp is repaired. Assign the camp builder to continue the loop.";
        public const string PLACE_STOCKPILE_HINT = "Place a stockpile so the settlement can hold expanded resources.";
        public const string PLACE_SHELTER_HINT = "Place a shelter to establish basic worker service.";
        public const string BRING_EXTRACTION_ONLINE_HINT = "Bring lumber or stone extraction online for steady supply.";
        public const string BRING_WORKBENCH_ONLINE_HINT = "Bring the workbench online to prepare the settlement economy.";

        private static readonly CampFlowStageDefinition[] DEFINITIONS =
        {
            new(CampFlowStage.DamagedCampStart, REPAIR_CAMP_OBJECTIVE, GATHER_REPAIR_RESOURCES_HINT, true),
            new(CampFlowStage.RepairObjectiveActive, REPAIR_CAMP_OBJECTIVE, GATHER_REPAIR_RESOURCES_HINT, true),
            new(CampFlowStage.RepairResourcesReady, REPAIR_CAMP_OBJECTIVE, CONTINUE_REPAIR_BUILD_HINT, false),
            new(CampFlowStage.CampRepaired, ASSIGN_WORKER_OBJECTIVE, ASSIGN_WORKER_HINT, false),
            new(CampFlowStage.WorkerAssigned, PLACE_STOCKPILE_OBJECTIVE, PLACE_STOCKPILE_HINT, false),
            new(CampFlowStage.StockpilePlaced, PLACE_SHELTER_OBJECTIVE, PLACE_SHELTER_HINT, false),
            new(CampFlowStage.ShelterPlaced, BRING_EXTRACTION_ONLINE_OBJECTIVE, BRING_EXTRACTION_ONLINE_HINT, false),
            new(CampFlowStage.ExtractionOnline, BRING_WORKBENCH_ONLINE_OBJECTIVE, BRING_WORKBENCH_ONLINE_HINT, false),
            new(CampFlowStage.WorkbenchOnline, PREPARE_BUILD_OBJECTIVE, string.Empty, false),
            new(CampFlowStage.LoadoutPrepared, PREPARE_BUILD_OBJECTIVE, string.Empty, false),
        };

        public static CampFlowStageDefinition Get(CampFlowStage stage)
        {
            for (var i = 0; i < DEFINITIONS.Length; i++)
            {
                if (DEFINITIONS[i].Stage == stage)
                    return DEFINITIONS[i];
            }

            throw new InvalidOperationException($"Camp flow stage definition is missing for {stage}.");
        }
    }
}
