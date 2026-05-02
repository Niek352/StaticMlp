namespace StaticMlp.Game.Bootstrap
{
    public abstract class GameplayFeature : IGameplayFeature
    {
        public virtual void RegisterPrefabs()
        {
        }

        public virtual void RegisterServerSystems(ServerSystemsBuilder systems)
        {
        }

        public virtual void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
        }
    }
}
