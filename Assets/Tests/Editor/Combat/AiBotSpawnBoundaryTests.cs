using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Shared;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class AiBotSpawnBoundaryTests
    {
        [Test]
        public void AiBotSpawns_Spawn_UsesExplicitSpecAndInitializesCombatAiState()
        {
            using var scope = new CombatTestServerWorldScope();

            var gid = SW.GetResource<AiBotFactory>().Spawn(new AiBotSpawnSpec(
                AiBotsGameplayFeature.BOT,
                new Vector3(4f, 0f, 6f),
                Quaternion.identity,
                behaviorId: 17,
                maxHealth: 150f,
                health01: 0.5f,
                hunger: 0.25f,
                fear: 0.75f,
                leader: default));

            Assert.That(gid.TryUnpack<ServerWT>(out var bot), Is.True);
            Assert.That(bot.Has<MonsterTag>(), Is.True);
            Assert.That(bot.Has<AiAgentTag>(), Is.True);
            Assert.That(bot.Read<AiBrain>().BehaviorId, Is.EqualTo(17));
            Assert.That(bot.Read<Health>().Current, Is.EqualTo(75f));
            Assert.That(bot.Read<Health>().Max, Is.EqualTo(150f));
            Assert.That(bot.Read<CharacterNetState>().Position, Is.EqualTo(new Vector3(4f, 0f, 6f)));
        }

        [Test]
        public void SettlementWorkerSpawner_Spawn_UsesExplicitSpecAndInitializesWorkerState()
        {
            using var scope = new CombatTestServerWorldScope();

            var gid = SW.GetResource<SettlementWorkerFactory>().Spawn(new SettlementWorkerSpawnSpec(
                SettlementAnchorCatalog.HomeCampId,
                WorkerRoleCatalog.CampBuilderId,
                SettlementWorkerNetworkArchetypeIds.CAMP_BUILDER_WORKER,
                behaviorId: 9,
                maxHealth: 120f,
                position: new Vector3(-2f, 0f, 3f),
                rotation: Quaternion.identity));

            Assert.That(gid.TryUnpack<ServerWT>(out var worker), Is.True);
            Assert.That(worker.Has<AiAgentTag>(), Is.True);
            Assert.That(worker.Has<SettlementWorkerTag>(), Is.True);
            Assert.That(worker.Read<SettlementWorkerIdentity>().HomeAnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId.Value));
            Assert.That(worker.Read<SettlementWorkerIdentity>().RoleId, Is.EqualTo(WorkerRoleCatalog.CampBuilderId.Value));
            Assert.That(worker.Read<AiBrain>().BehaviorId, Is.EqualTo(9));
            Assert.That(worker.Read<Health>().Max, Is.EqualTo(120f));
            Assert.That(worker.Read<CharacterNetState>().Position, Is.EqualTo(new Vector3(-2f, 0f, 3f)));
        }
    }
}
