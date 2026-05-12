using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public sealed class ServerFrontierEncounterBotSpawnSystem : ISystem
    {
        private EventReceiver<ServerWT, SpawnFrontierEncounterBotEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<SpawnFrontierEncounterBotEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
                Handle(in evt.Value);
        }

        private static void Handle(in SpawnFrontierEncounterBotEvent request)
        {
            var gid = SW.GetResource<AiBotFactory>().Spawn(new AiBotSpawnSpec(
                AiBotsGameplayFeature.BOT,
                request.Position,
                UnityEngine.Quaternion.identity,
                request.BehaviorId,
                maxHealth: 100f,
                request.Health01,
                request.Hunger,
                request.Fear,
                leader: default));

            if (!gid.TryUnpack<ServerWT>(out var entity))
                throw new System.InvalidOperationException("Spawned frontier encounter bot could not be unpacked in server world.");

            entity.Set(new FrontierEncounterParticipant
            {
                AnchorId = request.AnchorId,
                EncounterKind = request.EncounterKind,
                SourceId = request.SourceId
            });
        }
    }
}
