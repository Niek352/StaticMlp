using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;
using UnityEngine.Serialization;

namespace StaticMlp.Features.AiBots
{
    public sealed class BotNavigationGizmoPart : MonoBehaviour, IEntityViewPart
    {
        [FormerlySerializedAs("showInPlayModeOnly")] [SerializeField] private bool _showInPlayModeOnly = true;
        [FormerlySerializedAs("pathColor")] [SerializeField] private Color _pathColor = new(0.18f, 0.9f, 0.95f, 1f);
        [FormerlySerializedAs("cornerColor")] [SerializeField] private Color _cornerColor = new(0.95f, 0.82f, 0.28f, 1f);
        [FormerlySerializedAs("goalColor")] [SerializeField] private Color _goalColor = new(1f, 0.4f, 0.2f, 1f);
        [FormerlySerializedAs("cornerRadius")] [SerializeField] private float _cornerRadius = 0.14f;
        [FormerlySerializedAs("goalRadius")] [SerializeField] private float _goalRadius = 0.2f;

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

        public bool TryGetBoundBotGid(out EntityGID gid)
        {
            gid = _gid;
            return _isBound;
        }

        private void OnDrawGizmos()
        {
            if (!_isBound)
                return;

            if (_showInPlayModeOnly && !Application.isPlaying)
                return;

            if (SW.Status != WorldStatus.Initialized)
                return;

            DrawNavigationGizmos();
        }

        private void DrawNavigationGizmos()
        {
            if (!SW.HasResource<AiNavigationRuntime>())
                return;

            var runtime = SW.GetResource<AiNavigationRuntime>();
            if (runtime == null || !runtime.TryGetDebugState(_gid, out var debugState) || !debugState.HasGoal)
                return;

            var corners = debugState.PathCorners;
            var drewPath = corners != null && corners.Length > 0;
            if (drewPath)
            {
                Gizmos.color = _pathColor;
                for (var i = 0; i < corners.Length - 1; i++)
                    Gizmos.DrawLine(corners[i], corners[i + 1]);

                Gizmos.color = _cornerColor;
                for (var i = 0; i < corners.Length; i++)
                    Gizmos.DrawSphere(corners[i], _cornerRadius);
            }

            if (!drewPath && _gid.TryUnpack<ServerWT>(out var entity) && entity.Has<CharacterNetState>())
            {
                var position = entity.Read<CharacterNetState>().Position;
                Gizmos.color = _pathColor;
                Gizmos.DrawLine(position, debugState.Goal);
            }

            Gizmos.color = _goalColor;
            Gizmos.DrawWireSphere(debugState.Goal, _goalRadius);
        }
    }
}
