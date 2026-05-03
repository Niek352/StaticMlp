using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ServerResourcesInventorySeedSystem : ISystem
    {
        private readonly int _startingWood;
        private readonly int _startingStone;

        public ServerResourcesInventorySeedSystem(int startingWood = 50, int startingStone = 25)
        {
            _startingWood = startingWood;
            _startingStone = startingStone;
        }

        public void Update()
        {
            foreach (var e in SW.Query<All<PlayerTag, NetworkIdentity>, None<ResourcesInventory>>().Entities())
            {
                e.Set(new ResourcesInventory
                {
                    Wood = _startingWood,
                    Stone = _startingStone
                });
            }
        }
    }
}
