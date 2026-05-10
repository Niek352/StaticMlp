using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Statuses;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class ClientCombatResultReconcileSystemTests
    {
        [Test]
        public void Update_FromDamageNumberEvent_ConfirmsPredictedCommand()
        {
            using var scope = new CombatTestClientWorldScope();
            var player = scope.CreateLocalPlayer(Vector3.zero);
            player.Set(new LocalCombatPredictionState
            {
                LastPredictedCommandId = 77u,
            });

            var system = new ClientCombatResultReconcileSystem();
            system.Init();
            CW.SendEvent(new NetworkEventFromServer<DamageNumberEvent>(
                new NetworkPeerId(1),
                new DamageNumberEvent
                {
                    Source = player.GID,
                    Target = default,
                    ClientCommandId = 77u,
                    Value = 12f,
                    DamageType = DamageType.Fire,
                }));

            system.Update();
            system.Destroy();

            ref readonly var prediction = ref player.Read<LocalCombatPredictionState>();
            Assert.That(prediction.LastConfirmedCommandId, Is.EqualTo(77u));
            Assert.That(prediction.LastResolvedDamage, Is.EqualTo(12f));
        }
    }
}
