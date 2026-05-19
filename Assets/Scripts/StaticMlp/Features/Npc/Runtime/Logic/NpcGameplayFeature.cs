using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Npc
{
    public sealed class NpcGameplayFeature : GameplayFeature
    {
        public const ushort NPC_ROSTER_RECORD = 500;

        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<NpcRosterRecord>();

            RequestRegistry.Register<ExtractNpcRequestEvent, ExtractNpcResultEvent>(
                new ExtractNpcHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 40);

            RequestRegistry.Register<RescueNpcRequestEvent, RescueNpcResultEvent>(
                new RescueNpcHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 39);
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(NPC_ROSTER_RECORD, e =>
            {
                e.Set<NpcRosterRecordTag>();
                e.Set(new NpcRosterRecord());
            });

            NetArchetypeRegistry.RegisterServer(NPC_ROSTER_RECORD, e =>
            {
                e.Set<NpcRosterRecordTag>();
                e.Set(new NpcRosterRecord());
            });
        }

        public override void RegisterServerResources()
        {
            NpcDefinitionCatalogValidator.Validate(NpcDefinitionCatalog.All);
            NpcIncubationRecipeCatalogValidator.Validate(NpcIncubationRecipeCatalog.All);
            SW.SetResource(new NpcRosterRecordFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerNpcIncubationJobCompleteSystem(), GameplaySystemOrder.Gameplay - 38);
        }
    }
}
