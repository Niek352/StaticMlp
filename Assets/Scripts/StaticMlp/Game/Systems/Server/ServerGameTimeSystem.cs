using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server
{
    public sealed class ServerGameTimeSystem : ISystem
    {
        public void Init()
        {
            SW.SetResource(new GameTime());
        }

        public void Update()
        {
            var gameTime = SW.GetResource<GameTime>();
            gameTime.Time = Time.time;
            gameTime.DeltaTime = Time.deltaTime;
        }
    }
}
