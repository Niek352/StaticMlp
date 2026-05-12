using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Player;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientStage1ContextPanelSessionSystem : ISystem
    {
        private readonly float _focusRange;

        public ClientStage1ContextPanelSessionSystem(float focusRange = 4f)
        {
            _focusRange = focusRange;
        }

        public void Update()
        {
            ref var session = ref CW.GetResource<Stage1ContextPanelSession>();
            session.WorkerAnchorId = SettlementAnchorCatalog.HomeCampId;
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var stage = anchor.Read<Stage1SettlementProgression>().Stage;

            if (stage < Stage1SettlementProgressStage.CampRepaired)
            {
                if (!TryFindRepairFocusSite(out var repairSite))
                {
                    return;
                    throw new InvalidOperationException(
                        $"Stage 1 repair flow requires a construction-site repair target for anchor {SettlementAnchorCatalog.HomeCampId.Value}.");
                }

                session.Mode = Stage1ContextPanelMode.Building;
                session.FocusedSite = repairSite.GID;
                return;
            }

            if (!ClientLocalPlayer.TryGetPosition(out var playerPosition))
            {
                session.Mode = Stage1ContextPanelMode.Worker;
                session.FocusedSite = default;
                return;
            }

            if (TryFindNearestSite(playerPosition, out var site))
            {
                session.Mode = Stage1ContextPanelMode.Building;
                session.FocusedSite = site.GID;
                return;
            }

            session.Mode = Stage1ContextPanelMode.Worker;
            session.FocusedSite = default;
        }

        private bool TryFindRepairFocusSite(out CW.Entity site)
        {
            if (TryFindAnchorRepairSite(SettlementAnchorCatalog.HomeCampId, out site))
                return true;

            if (ClientLocalPlayer.TryGetPosition(out var playerPosition)
                && TryFindNearestSite(playerPosition, out site))
            {
                return true;
            }

            return false;
        }

        private bool TryFindNearestSite(Vector3 playerPosition, out CW.Entity site)
        {
            var bestDistanceSq = _focusRange * _focusRange;
            var found = false;
            site = default;

            foreach (var entity in CW.Query<All<ConstructionTransform, ConstructionSiteState>>().Entities())
            {
                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(entity);
                if (state.Phase == ConstructionPhase.Completed)
                    continue;

                var distanceSq = (entity.Read<ConstructionTransform>().Position - playerPosition).sqrMagnitude;
                if (distanceSq > bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                site = entity;
                found = true;
            }

            return found;
        }

        private static bool TryFindAnchorRepairSite(SettlementAnchorId anchorId, out CW.Entity site)
        {
            foreach (var entity in CW.Query<All<ConstructionSiteState, SettlementAnchorRef>>().Entities())
            {
                ref readonly var anchorRef = ref ClientProjection.Read<SettlementAnchorRef>(entity);
                if (anchorRef.AnchorId != anchorId.Value)
                    continue;

                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(entity);
                if (state.Phase == ConstructionPhase.Completed)
                    continue;

                site = entity;
                return true;
            }

            site = default;
            return false;
        }
    }
}
