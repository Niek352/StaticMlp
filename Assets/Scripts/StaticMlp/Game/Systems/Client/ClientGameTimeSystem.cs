using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Game.Systems.Client
{
    public sealed class ClientGameTimeSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new GameTime());
        }

        public void Update()
        {
            var gameTime = CW.GetResource<GameTime>();
            gameTime.Time = Time.time;
            gameTime.DeltaTime = Time.deltaTime;
        }
    }
}
