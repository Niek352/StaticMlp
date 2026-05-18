using System;
using NUnit.Framework;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Npc;
using StaticMlp.Game;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcIncubationJobTests
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
        public void JobCompleteSystem_ReadyJob_CreatesRosterRecord()
        {
            var recipe = NpcIncubationRecipeCatalog.Get(NpcIncubationRecipeCatalog.ResearcherRecipeId);
            var entity = SW.NewEntity<Default>();
            entity.Set(new NpcIncubationJobState
            {
                RecipeId = recipe.Id.Value,
                State = NpcRosterState.Incubating,
                StartedAtServerTick = 0,
                CompletesAtServerTick = 10
            });

            _scope.SimulationTime.ServerTick = 10;
            new ServerNpcIncubationJobCompleteSystem().Update();

            Assert.That(entity.Has<NpcIncubationJobState>(), Is.False);
        }

        [Test]
        public void JobCompleteSystem_IncompleteJob_DoesNotComplete()
        {
            var recipe = NpcIncubationRecipeCatalog.Get(NpcIncubationRecipeCatalog.ResearcherRecipeId);
            var entity = SW.NewEntity<Default>();
            entity.Set(new NpcIncubationJobState
            {
                RecipeId = recipe.Id.Value,
                State = NpcRosterState.Incubating,
                StartedAtServerTick = 0,
                CompletesAtServerTick = 100
            });

            _scope.SimulationTime.ServerTick = 50;
            new ServerNpcIncubationJobCompleteSystem().Update();

            Assert.That(entity.Has<NpcIncubationJobState>(), Is.True);
        }
    }
}
