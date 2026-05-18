using System;
using NUnit.Framework;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcExtractionTests
    {
        private NpcTestServerWorldScope _scope;

        [SetUp]
        public void SetUp()
        {
            _scope = new NpcTestServerWorldScope();
        }

        [TearDown]
        public void TearDown()
        {
            _scope?.Dispose();
        }

        [Test]
        public void Extract_NonExtractableTarget_IsRejected()
        {
            var target = CreateTarget(extractable: false);
            var handler = new ExtractNpcHandler();
            var request = new ExtractNpcRequestEvent(target.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Extract_ExpiredExtractableTarget_IsRejected()
        {
            var target = CreateTarget(extractable: true, expiresAtTick: 5);
            _scope.SimulationTime.ServerTick = 10;

            var handler = new ExtractNpcHandler();
            var request = new ExtractNpcRequestEvent(target.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Extract_DefinitionNotAllowedByExtraction_IsRejected()
        {
            var target = CreateTarget(
                extractable: true,
                definitionId: NpcDefinitionCatalog.RescuedSpecialistId.Value);

            var handler = new ExtractNpcHandler();
            var request = new ExtractNpcRequestEvent(target.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Extract_DeadTarget_IsRejected()
        {
            var target = CreateTarget(extractable: true, dead: true);

            var handler = new ExtractNpcHandler();
            var request = new ExtractNpcRequestEvent(target.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Rejected));
        }

        [Test]
        public void Extract_ValidTarget_CreatesCapturedRosterRecord()
        {
            var target = CreateTarget(extractable: true);

            var handler = new ExtractNpcHandler();
            var request = new ExtractNpcRequestEvent(target.GID);

            var result = handler.Handle(new NetworkPeerId(1), request);

            Assert.That(result.AcquisitionResult, Is.EqualTo(NpcAcquisitionResult.Accepted));
            Assert.That(result.RosterRecord, Is.Not.EqualTo(default(EntityGID)));
            Assert.That(result.Status, Is.EqualTo(Networking.Requests.RequestStatus.Accepted));
        }

        [Test]
        public void Extract_RequestUsesEntityGid()
        {
            var target = CreateTarget(extractable: true);
            var request = new ExtractNpcRequestEvent(target.GID);

            Assert.That(request.Target, Is.EqualTo(target.GID));
            Assert.That(request.Target, Is.Not.EqualTo(default(EntityGID)));
        }

        private SW.Entity CreateTarget(
            bool extractable,
            ushort definitionId = 0,
            uint expiresAtTick = 1000,
            bool dead = false)
        {
            if (definitionId == 0)
                definitionId = NpcDefinitionCatalog.ExtractedCompanionId.Value;

            var entity = SW.NewEntity<Default>();
            entity.Set<ServerOwned>();
            entity.Set(new CharacterNetState
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity
            });

            if (extractable)
            {
                entity.Set(new ExtractableState
                {
                    NpcDefinitionId = definitionId,
                    ExpiresAtServerTick = expiresAtTick
                });
                entity.Set<ExtractionTargetTag>();
            }

            if (dead)
            {
                entity.Set(new Health { Current = 0f, Max = 100f });
            }
            else
            {
                entity.Set(new Health { Current = 100f, Max = 100f });
            }

            return entity;
        }
    }

    public sealed class NpcTestServerWorldScope : IDisposable
    {
        public NpcTestServerWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            NetworkEventRegistry.Clear();
            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(NpcGameplayFeature).Assembly,
                typeof(Health).Assembly,
                typeof(PlayerTag).Assembly,
                typeof(CharacterNetState).Assembly);
            SW.Initialize();
            NetworkEventRegistry.RegisterServerWorldTypes();
            ProjectionRegistry.Register<NpcRosterRecord>();

            SW.SetResource(new SimulationTime
            {
                FixedStepSeconds = 1f / 30f,
                ServerTick = 100
            });
            SW.SetResource(new NpcRosterRecordFactory());

            var player = SW.NewEntity<Default>();
            player.Set<PlayerTag>();
            player.Set(new NetworkIdentity
            {
                Owner = new NetworkPeerId(1),
                Authority = NetworkAuthority.Owner,
                NetworkArchetypeId = 1
            });
            player.Set<NetworkedTag>();
            player.Set(new CharacterNetState
            {
                Position = Vector3.zero,
                Velocity = Vector3.zero,
                Rotation = Quaternion.identity
            });
        }

        public SimulationTime SimulationTime => SW.GetResource<SimulationTime>();

        public void Dispose()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
