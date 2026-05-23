using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Interaction;
using StaticMlp.Features.Player;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementContextPanelSessionSystem : ISystem
    {
        private readonly float _focusRange;

        public ClientSettlementContextPanelSessionSystem(float focusRange = 4f)
        {
            _focusRange = focusRange;
        }

        public void Update()
        {
            ref var session = ref CW.GetResource<SettlementContextPanelSession>();
            session.WorkerAnchorId = SettlementAnchorCatalog.HomeCampId;
            if (!CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var stage = anchor.Read<CampFlowProgression>().Stage;

            if (TryFindExplicitFocusSite(out var focusedSite))
            {
                session.Mode = SettlementContextPanelMode.Building;
                session.FocusedSite = focusedSite.GID;
                return;
            }

            if (stage < CampFlowStage.CampRepaired)
            {
                if (!TryFindRepairFocusSite(out var repairSite))
                {
                    if (HasCompletedAnchorConstruction(SettlementAnchorCatalog.HomeCampId))
                    {
                        session.Mode = SettlementContextPanelMode.Worker;
                        session.FocusedSite = default;
                        return;
                    }

                    throw new InvalidOperationException(
                        $"Stage 1 repair flow requires a construction-site repair target for anchor {SettlementAnchorCatalog.HomeCampId.Value}.");
                }

                session.Mode = SettlementContextPanelMode.Building;
                session.FocusedSite = repairSite.GID;
                return;
            }

            if (!ClientLocalPlayer.TryGetPosition(out _))
            {
                session.Mode = SettlementContextPanelMode.Worker;
                session.FocusedSite = default;
                return;
            }

            ref readonly var focus = ref CW.GetResource<InteractionFocus>();
            if (focus.HasFocus
                && (focus.Kind == InteractableKind.ConstructionSite
                    || focus.Kind == InteractableKind.FinishedBuilding))
            {
                session.Mode = SettlementContextPanelMode.Building;
                session.FocusedSite = focus.Target;
                return;
            }

            session.Mode = SettlementContextPanelMode.Worker;
            session.FocusedSite = default;
        }

        private static bool TryFindExplicitFocusSite(out CW.Entity site)
        {
            ref readonly var focus = ref CW.GetResource<SettlementContextFocusTarget>();
            if (!focus.HasTarget)
            {
                site = default;
                return false;
            }

            if (!focus.Target.TryUnpack<ClientCoreWT>(out site))
                throw new InvalidOperationException(
                    $"Stage 1 context focus target {focus.Target.Raw} does not exist in the client world.");

            if (!site.Has<ConstructionSiteState>())
                throw new InvalidOperationException(
                    $"Stage 1 context focus target {focus.Target.Raw} is not a construction-site entity.");

            return true;
        }

        private bool TryFindRepairFocusSite(out CW.Entity site)
        {
            if (TryFindAnchorRepairSite(SettlementAnchorCatalog.HomeCampId, out site))
                return true;

            if (ClientLocalPlayer.TryGetPosition(out var playerPosition)
                && TryFindNearestSite(playerPosition, includeCompleted: false, out site))
            {
                return true;
            }

            return false;
        }

        private bool TryFindNearestSite(Vector3 playerPosition, bool includeCompleted, out CW.Entity site)
        {
            var bestDistanceSq = _focusRange * _focusRange;
            var found = false;
            site = default;

            foreach (var entity in CW.Query<All<ConstructionTransform, ConstructionSiteState>>().Entities())
            {
                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(entity);
                if (!includeCompleted && state.Phase == ConstructionPhase.Completed)
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

        private static bool HasCompletedAnchorConstruction(SettlementAnchorId anchorId)
        {
            foreach (var entity in CW.Query<All<ConstructionSiteState, SettlementAnchorRef>>().Entities())
            {
                ref readonly var anchorRef = ref ClientProjection.Read<SettlementAnchorRef>(entity);
                if (anchorRef.AnchorId != anchorId.Value)
                    continue;

                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(entity);
                if (state.Phase == ConstructionPhase.Completed)
                    return true;
            }

            return false;
        }
    }
}
