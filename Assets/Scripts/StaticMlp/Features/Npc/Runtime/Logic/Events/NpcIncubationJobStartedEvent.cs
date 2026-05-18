using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public readonly struct NpcIncubationJobStartedEvent : IEvent, IEventConfig<NpcIncubationJobStartedEvent>
    {
        public readonly EntityGID JobEntity;
        public readonly NpcIncubationRecipeId RecipeId;

        public NpcIncubationJobStartedEvent(EntityGID jobEntity, NpcIncubationRecipeId recipeId)
        {
            JobEntity = jobEntity;
            RecipeId = recipeId;
        }

        public EventTypeConfig<NpcIncubationJobStartedEvent> Config() =>
            new(guid: new Guid("c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f"));
    }
}
