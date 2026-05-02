namespace StaticMlp.Networking.Ownership {
    public static class OwnershipTags {
        public static void ApplyForClient(StaticMlp.Networking.CW.Entity e, NetworkPeerId owner, NetworkAuthority authority) {
            if (e.Has<LocalOwned>()) e.Delete<LocalOwned>();
            if (e.Has<RemoteOwned>()) e.Delete<RemoteOwned>();
            if (e.Has<ServerOwned>()) e.Delete<ServerOwned>();
            if (e.Has<ClientOwned>()) e.Delete<ClientOwned>();

            if (authority == NetworkAuthority.Owner && owner == NetworkRuntime.LocalPeerId) {
                e.Set<LocalOwned>();
                return;
            }

            e.Set<RemoteOwned>();
        }

        public static void ApplyForServer(StaticMlp.Networking.SW.Entity e, NetworkPeerId owner, NetworkAuthority authority) {
            if (e.Has<ClientOwned>()) e.Delete<ClientOwned>();
            if (e.Has<ServerOwned>()) e.Delete<ServerOwned>();
            if (e.Has<LocalOwned>()) e.Delete<LocalOwned>();
            if (e.Has<RemoteOwned>()) e.Delete<RemoteOwned>();

            if (authority == NetworkAuthority.Server) {
                e.Set<ServerOwned>();
                return;
            }

            if (authority == NetworkAuthority.Owner)
                e.Set<ClientOwned>();
        }
    }
}
