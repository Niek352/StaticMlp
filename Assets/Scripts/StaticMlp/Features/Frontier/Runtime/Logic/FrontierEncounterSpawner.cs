using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Frontier
{
    public static class FrontierEncounterSpawner
    {
        public static void SpawnEncounter(
            ushort anchorId,
            FrontierEncounterKind encounterKind,
            ushort sourceId,
            EncounterProfileId encounterProfileId,
            Vector3 origin)
        {
            var encounterProfile = EncounterProfileCatalog.Get(encounterProfileId);
            for (var i = 0; i < encounterProfile.HostileSpawns.Length; i++)
            {
                var hostile = encounterProfile.HostileSpawns[i];
                SW.SendEvent(new SpawnFrontierEncounterBotEvent
                {
                    Position = origin + hostile.Offset,
                    BehaviorId = hostile.BehaviorId == 0 ? CombatEnemyBehaviorIds.Default : hostile.BehaviorId,
                    Health01 = hostile.Health01,
                    Hunger = hostile.Hunger,
                    Fear = hostile.Fear,
                    AnchorId = anchorId,
                    EncounterKind = encounterKind,
                    SourceId = sourceId
                });
            }
        }
    }
}
