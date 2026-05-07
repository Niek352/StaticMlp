using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiBots
{
    public sealed class BotNavigationGizmoPart : MonoBehaviour, IEntityViewPart
    {
        [SerializeField] private bool showInPlayModeOnly = true;
        [SerializeField] private Color pathColor = new(0.18f, 0.9f, 0.95f, 1f);
        [SerializeField] private Color cornerColor = new(0.95f, 0.82f, 0.28f, 1f);
        [SerializeField] private Color goalColor = new(1f, 0.4f, 0.2f, 1f);
        [SerializeField] private float cornerRadius = 0.14f;
        [SerializeField] private float goalRadius = 0.2f;

        private EntityGID _gid;
        private bool _isBound;

        public void OnBind(IEntityView view)
        {
            _gid = view.Entity.GID;
            _isBound = true;
        }

        public void OnUnbind()
        {
            _gid = default;
            _isBound = false;
        }

        private void OnDrawGizmos()
        {
            if (!_isBound)
                return;

            if (showInPlayModeOnly && !Application.isPlaying)
                return;

            if (SW.Status != WorldStatus.Initialized || !SW.HasResource<AiNavigationRuntime>())
                return;

            var runtime = SW.GetResource<AiNavigationRuntime>();
            if (runtime == null || !runtime.TryGetDebugState(_gid, out var debugState) || !debugState.HasGoal)
                return;

            var corners = debugState.PathCorners;
            var drewPath = corners != null && corners.Length > 0;
            if (drewPath)
            {
                Gizmos.color = pathColor;
                for (var i = 0; i < corners.Length - 1; i++)
                    Gizmos.DrawLine(corners[i], corners[i + 1]);

                Gizmos.color = cornerColor;
                for (var i = 0; i < corners.Length; i++)
                    Gizmos.DrawSphere(corners[i], cornerRadius);
            }

            if (!drewPath && _gid.TryUnpack<ServerWT>(out var entity) && entity.Has<CharacterNetState>())
            {
                var position = entity.Read<CharacterNetState>().Position;
                Gizmos.color = pathColor;
                Gizmos.DrawLine(position, debugState.Goal);
            }

            Gizmos.color = goalColor;
            Gizmos.DrawWireSphere(debugState.Goal, goalRadius);
        }
    }
}
