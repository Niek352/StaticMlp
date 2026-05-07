using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Game.Requests
{
    public static class RequestRuntimeBootstrap
    {
        private const short CLIENT_REQUEST_RUNTIME_INIT_ORDER = 190;
        private const short CLIENT_PROJECTION_REBUILD_ORDER = 210;

        public static void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            foreach (var registration in RequestRegistry.Registrations)
                systems.Add(registration.CreateServerSystem(), registration.ServerOrder);
        }

        public static void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            if (!RequestRegistry.HasRegistrations)
                return;

            systems.Add(new ClientRequestRuntimeInitSystem(), CLIENT_REQUEST_RUNTIME_INIT_ORDER);

            foreach (var registration in RequestRegistry.Registrations)
                systems.Add(registration.CreateClientResultSystem(), registration.ClientResultOrder);

            if (ProjectionRegistry.HasRegistrations)
                systems.Add(new ClientProjectionRebuildSystem(), CLIENT_PROJECTION_REBUILD_ORDER);
        }
    }
}
