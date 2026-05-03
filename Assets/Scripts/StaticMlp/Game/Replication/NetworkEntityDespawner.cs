namespace StaticMlp.Networking.Replication
{
    public static class NetworkEntityDespawner
    {
        public static void DespawnAndDestroy(SW.Entity entity)
        {
            DespawnBroadcaster.SendDespawn(entity.GID);
            entity.Destroy();
        }
    }
}
