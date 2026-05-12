namespace StaticMlp.Game.Bootstrap
{
    public interface IGameplayFeature
    {
        void RegisterNetworkEvents();
        void RegisterPrefabs();
        void RegisterServerResources();
        void RegisterServerSystems(ServerSystemsBuilder systems);
        void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems);
        void RegisterClientViewSync(ViewSyncBuilder views);
    }
}
