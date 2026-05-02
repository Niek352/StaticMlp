namespace StaticMlp.Game.Bootstrap
{
    public interface IGameplayFeature
    {
        void RegisterPrefabs();
        void RegisterServerSystems(ServerSystemsBuilder systems);
        void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems);
    }
}
