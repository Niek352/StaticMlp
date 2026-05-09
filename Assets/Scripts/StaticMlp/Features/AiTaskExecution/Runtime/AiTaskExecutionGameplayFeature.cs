using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.AiTaskExecution
{
    public sealed class AiTaskExecutionGameplayFeature : GameplayFeature
    {
        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerAiTaskExecutionSystem(), GameplaySystemOrder.Gameplay - 40);
        }
    }
}
