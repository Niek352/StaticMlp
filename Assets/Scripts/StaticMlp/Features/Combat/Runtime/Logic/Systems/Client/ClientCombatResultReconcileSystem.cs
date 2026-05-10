using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatResultReconcileSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<DamageNumberEvent>> _damageResults;
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<DeathEvent>> _deathResults;

        public void Init()
        {
            _damageResults = CW.RegisterEventReceiver<NetworkEventFromServer<DamageNumberEvent>>();
            _deathResults = CW.RegisterEventReceiver<NetworkEventFromServer<DeathEvent>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _damageResults);
            CW.DeleteEventReceiver(ref _deathResults);
        }

        public void Update()
        {
            foreach (var evt in _damageResults)
                ApplyDamageResult(in evt.Value.Value);

            foreach (var evt in _deathResults)
                ApplyDeathResult(in evt.Value.Value);
        }

        private static void ApplyDamageResult(in DamageNumberEvent evt)
        {
            if (!evt.Source.TryUnpack<ClientCoreWT>(out var source) || !source.Has<LocalCombatPredictionState>())
                return;

            ref var prediction = ref source.Mut<LocalCombatPredictionState>();
            prediction.LastConfirmedCommandId = evt.ClientCommandId;
            prediction.LastResolvedDamage = evt.Value;
        }

        private static void ApplyDeathResult(in DeathEvent evt)
        {
            if (!evt.Source.TryUnpack<ClientCoreWT>(out var source) || !source.Has<LocalCombatPredictionState>())
                return;

            ref var prediction = ref source.Mut<LocalCombatPredictionState>();
            prediction.LastConfirmedCommandId = evt.ClientCommandId;
        }
    }
}
