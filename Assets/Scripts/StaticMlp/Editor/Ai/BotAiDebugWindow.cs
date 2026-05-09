using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using UnityEditor;
using UnityEngine;

namespace StaticMlp.Editor.Ai
{
    public sealed class BotAiDebugWindow : EditorWindow
    {
        private const float REPAINT_INTERVAL = 0.2f;
        private const float TASK_NAME_WIDTH = 170f;
        private const float TASK_SCORE_WIDTH = 72f;
        private const float TASK_FLAG_WIDTH = 56f;
        private const float CONSIDERATION_NAME_WIDTH = 170f;
        private const float CONSIDERATION_NUMBER_WIDTH = 68f;
        private const float CONSIDERATION_CURVE_WIDTH = 120f;

        private readonly GUIContent _trackSelectionLabel = new("Track Selection");
        private readonly GUIContent _useSelectionLabel = new("Use Selection");

        private Vector2 _scroll;
        private double _lastRepaint;
        private bool _trackSelection = true;
        private BotNavigationGizmoPart _target;
        private int _selectedTaskIndex = -1;

        [MenuItem("Window/StaticMlp/AI Bot Debug")]
        public static void OpenWindow()
        {
            var window = GetWindow<BotAiDebugWindow>("AI Bot Debug");
            window.minSize = new Vector2(560f, 360f);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            EditorApplication.update += RepaintOnInterval;
            Selection.selectionChanged += OnSelectionChanged;
            SyncSelection();
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintOnInterval;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            if (_trackSelection)
                SyncSelection();

            Repaint();
        }

        private void RepaintOnInterval()
        {
            if (!Application.isPlaying)
                return;

            if (EditorApplication.timeSinceStartup - _lastRepaint < REPAINT_INTERVAL)
                return;

            _lastRepaint = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6f);

            if (_target == null)
            {
                EditorGUILayout.HelpBox(
                    "Select a bot GameObject with BotNavigationGizmoPart in the Hierarchy, or assign it manually here.",
                    MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawTargetInfo();
            EditorGUILayout.Space(8f);
            DrawAiDebug();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var trackSelection = GUILayout.Toggle(
                _trackSelection,
                _trackSelectionLabel,
                EditorStyles.toolbarButton,
                GUILayout.Width(110f));

            if (trackSelection != _trackSelection)
            {
                _trackSelection = trackSelection;
                if (_trackSelection)
                    SyncSelection();
            }

            EditorGUI.BeginDisabledGroup(_trackSelection);
            var previousTarget = _target;
            _target = (BotNavigationGizmoPart)EditorGUILayout.ObjectField(
                _target,
                typeof(BotNavigationGizmoPart),
                true,
                GUILayout.MinWidth(180f));
            if (_target != previousTarget)
                _selectedTaskIndex = -1;
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(_useSelectionLabel, EditorStyles.toolbarButton, GUILayout.Width(100f)))
                SyncSelection();

            GUILayout.FlexibleSpace();
            GUILayout.Label(Application.isPlaying ? "play mode" : "edit mode", EditorStyles.miniLabel, GUILayout.Width(60f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTargetInfo()
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Bot View", _target, typeof(BotNavigationGizmoPart), true);

            if (!_target.TryGetBoundBotGid(out var gid))
            {
                EditorGUILayout.HelpBox("The selected bot view is not bound to a server entity right now.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Entity GID", gid.ToString());
        }

        private void DrawAiDebug()
        {
            EditorGUILayout.LabelField("Utility Breakdown", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect live AI utility state.", MessageType.Info);
                return;
            }

            if (SW.Status != WorldStatus.Initialized)
            {
                EditorGUILayout.HelpBox("Server world is not initialized yet.", MessageType.Warning);
                return;
            }

            if (!_target.TryGetBoundBotGid(out var gid))
            {
                EditorGUILayout.HelpBox("Bot entity binding is unavailable.", MessageType.Warning);
                return;
            }

            if (!gid.TryUnpack<ServerWT>(out var entity))
            {
                EditorGUILayout.HelpBox("Failed to resolve the selected bot in ServerWT.", MessageType.Warning);
                return;
            }

            if (!entity.Has<StaticMlp.Game.Components.CharacterNetState>() || !entity.Has<AiBrain>() || !entity.Has<AiTaskState>())
            {
                EditorGUILayout.HelpBox("Selected entity is missing AI runtime components.", MessageType.Warning);
                return;
            }

            if (!SW.HasResource<AiActionCatalog>())
            {
                EditorGUILayout.HelpBox("AiActionCatalog resource is missing in the server world.", MessageType.Warning);
                return;
            }

            var catalog = SW.GetResource<AiActionCatalog>();
            var brain = entity.Read<AiBrain>();
            var taskState = entity.Read<AiTaskState>();
            if (!catalog.TryGetBehavior(brain.BehaviorId, out var behavior))
            {
                EditorGUILayout.HelpBox(
                    $"Behavior '{brain.BehaviorId}' is not registered in AiActionCatalog.",
                    MessageType.Warning);
                return;
            }

            var snapshot = AiEditorDebugCalculator.BuildSnapshot(entity, catalog, behavior, brain, taskState);
            DrawSnapshotSummary(snapshot);
            EditorGUILayout.Space(8f);
            DrawTaskTable(snapshot);
            EditorGUILayout.Space(8f);
            DrawConsiderationTable(snapshot);
        }

        private void SyncSelection()
        {
            var nextTarget = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<BotNavigationGizmoPart>()
                : null;

            if (nextTarget != _target)
                _selectedTaskIndex = -1;

            _target = nextTarget;
        }

        private static void DrawSnapshotSummary(AiBotDebugSnapshot snapshot)
        {
            EditorGUILayout.LabelField("Position", snapshot.Position.ToString());
            EditorGUILayout.LabelField("Behavior", snapshot.BehaviorId.ToString());
            EditorGUILayout.LabelField("Current Task", snapshot.CurrentTask.ToString());
            EditorGUILayout.LabelField("Selected Task", snapshot.SelectedTask.ToString());
            EditorGUILayout.LabelField("Active Task", snapshot.HasActiveTask ? snapshot.ActiveTask.ToString() : "None");
        }

        private void DrawTaskTable(AiBotDebugSnapshot snapshot)
        {
            EditorGUILayout.LabelField("Tasks", EditorStyles.boldLabel);
            DrawTaskHeader();

            if (snapshot.Tasks == null || snapshot.Tasks.Length == 0)
            {
                EditorGUILayout.HelpBox("This behavior has no registered utility tasks.", MessageType.Info);
                return;
            }

            for (var i = 0; i < snapshot.Tasks.Length; i++)
            {
                var row = snapshot.Tasks[i];
                var isPicked = ResolveSelectedTaskIndex(snapshot) == i;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    var previousColor = GUI.backgroundColor;
                    if (row.IsBestTask)
                        GUI.backgroundColor = new Color(0.72f, 0.92f, 0.72f, 1f);
                    else if (isPicked)
                        GUI.backgroundColor = new Color(0.72f, 0.84f, 0.96f, 1f);

                    if (GUILayout.Toggle(isPicked, row.TaskType.ToString(), EditorStyles.miniButton, GUILayout.Width(TASK_NAME_WIDTH)))
                        _selectedTaskIndex = i;

                    GUI.backgroundColor = previousColor;
                    EditorGUILayout.LabelField(FormatFloat(row.Score), GUILayout.Width(TASK_SCORE_WIDTH));
                    EditorGUILayout.LabelField(FormatFlag(row.IsSelectedTask), GUILayout.Width(TASK_FLAG_WIDTH));
                    EditorGUILayout.LabelField(FormatFlag(row.IsActiveTask), GUILayout.Width(TASK_FLAG_WIDTH));
                    EditorGUILayout.LabelField(FormatFlag(row.IsBestTask), GUILayout.Width(TASK_FLAG_WIDTH));
                }
            }
        }

        private static void DrawTaskHeader()
        {
            using var header = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Task", EditorStyles.boldLabel, GUILayout.Width(TASK_NAME_WIDTH));
            EditorGUILayout.LabelField("Score", EditorStyles.boldLabel, GUILayout.Width(TASK_SCORE_WIDTH));
            EditorGUILayout.LabelField("Selected", EditorStyles.boldLabel, GUILayout.Width(TASK_FLAG_WIDTH));
            EditorGUILayout.LabelField("Active", EditorStyles.boldLabel, GUILayout.Width(TASK_FLAG_WIDTH));
            EditorGUILayout.LabelField("Best", EditorStyles.boldLabel, GUILayout.Width(TASK_FLAG_WIDTH));
        }

        private void DrawConsiderationTable(AiBotDebugSnapshot snapshot)
        {
            EditorGUILayout.LabelField("Considerations", EditorStyles.boldLabel);

            if (snapshot.Tasks == null || snapshot.Tasks.Length == 0)
                return;

            var selectedIndex = ResolveSelectedTaskIndex(snapshot);
            if (selectedIndex < 0 || selectedIndex >= snapshot.Tasks.Length)
            {
                EditorGUILayout.HelpBox("No task is available for detailed inspection.", MessageType.Info);
                return;
            }

            var task = snapshot.Tasks[selectedIndex];
            EditorGUILayout.LabelField("Task", task.TaskType.ToString());
            EditorGUILayout.LabelField("Score", FormatFloat(task.Score));
            DrawConsiderationHeader();

            if (task.Considerations == null || task.Considerations.Length == 0)
            {
                EditorGUILayout.HelpBox("The selected task has no considerations, so its score is 0.", MessageType.Info);
                return;
            }

            for (var i = 0; i < task.Considerations.Length; i++)
            {
                var row = task.Considerations[i];
                using var line = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox);
                EditorGUILayout.LabelField(row.VariableName, GUILayout.Width(CONSIDERATION_NAME_WIDTH));
                EditorGUILayout.LabelField(FormatFloat(row.RawValue), GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
                EditorGUILayout.LabelField(row.Curve.ToString(), GUILayout.Width(CONSIDERATION_CURVE_WIDTH));
                EditorGUILayout.LabelField(FormatFloat(row.CurvedValue), GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
                EditorGUILayout.LabelField(FormatFloat(row.Weight), GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
                EditorGUILayout.LabelField(FormatFloat(row.Factor), GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
            }
        }

        private static void DrawConsiderationHeader()
        {
            using var header = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Variable", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_NAME_WIDTH));
            EditorGUILayout.LabelField("Raw", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
            EditorGUILayout.LabelField("Curve", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_CURVE_WIDTH));
            EditorGUILayout.LabelField("Curved", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
            EditorGUILayout.LabelField("Weight", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
            EditorGUILayout.LabelField("Factor", EditorStyles.boldLabel, GUILayout.Width(CONSIDERATION_NUMBER_WIDTH));
        }

        private int ResolveSelectedTaskIndex(AiBotDebugSnapshot snapshot)
        {
            if (snapshot.Tasks == null || snapshot.Tasks.Length == 0)
                return -1;

            if (_selectedTaskIndex >= 0 && _selectedTaskIndex < snapshot.Tasks.Length)
                return _selectedTaskIndex;

            if (snapshot.BestTaskIndex >= 0 && snapshot.BestTaskIndex < snapshot.Tasks.Length)
                return snapshot.BestTaskIndex;

            return 0;
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.000");
        }

        private static string FormatFlag(bool value)
        {
            return value ? "Yes" : "-";
        }
    }
}
