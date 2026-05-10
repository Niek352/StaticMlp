using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientPassiveAutoAttackSendSystem : ISystem
    {
        public void Update()
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState, PassiveAutoAttackState, PassiveAutoAttackIntent>>().Entities())
            {
                ref var state = ref player.Mut<PassiveAutoAttackState>();
                ref readonly var intent = ref player.Read<PassiveAutoAttackIntent>();
                if (intent.ShotSequence <= state.LastSentShotSequence)
                    continue;

                var request = new UseAbilityCommand(
                    intent.AbilityId,
                    intent.Target,
                    intent.ShotSequence);
                if (!CW.SendToServerEvent(in request))
                    continue;

                state.LastSentShotSequence = intent.ShotSequence;
            }
        }
    }
}
