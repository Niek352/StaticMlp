namespace StaticMlp.Networking.Ownership
{
    public static class NetworkEntityOwnership
    {
        public static bool TryGetOwner(SW.Entity entity, out NetworkPeerId owner)
        {
            if (entity.Has<NetworkIdentity>())
            {
                owner = entity.Read<NetworkIdentity>().Owner;
                return true;
            }

            owner = default;
            return false;
        }

        public static bool TryGetOwner(CW.Entity entity, out NetworkPeerId owner)
        {
            if (entity.Has<NetworkIdentity>())
            {
                owner = entity.Read<NetworkIdentity>().Owner;
                return true;
            }

            owner = default;
            return false;
        }

        public static bool IsOwnedBy(SW.Entity entity, NetworkPeerId peer)
        {
            return entity.Has<NetworkIdentity>()
                   && entity.Read<NetworkIdentity>().Owner == peer;
        }

        public static bool IsOwnedBy(CW.Entity entity, NetworkPeerId peer)
        {
            return entity.Has<NetworkIdentity>()
                   && entity.Read<NetworkIdentity>().Owner == peer;
        }
    }
}
