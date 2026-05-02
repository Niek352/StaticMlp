using System;
using StaticMlp.Networking.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace StaticMlp.Editor.NetworkDiagnostics {
    public sealed class NetworkTrafficWindow : EditorWindow {
        private const float RepaintInterval = 0.25f;
        private Vector2 _scroll;
        private double _lastRepaint;
        private double _windowSeconds = 5d;

        [MenuItem("Window/StaticMlp/Network Traffic")]
        public static void OpenWindow() {
            var window = GetWindow<NetworkTrafficWindow>("Network Traffic");
            window.minSize = new Vector2(720f, 420f);
            window.Show();
            window.Focus();
        }

        private void OnEnable() {
            EditorApplication.update += RepaintOnInterval;
        }

        private void OnDisable() {
            EditorApplication.update -= RepaintOnInterval;
        }

        private void RepaintOnInterval() {
            if (EditorApplication.timeSinceStartup - _lastRepaint < RepaintInterval)
                return;

            _lastRepaint = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void OnGUI() {
            var snapshot = NetworkTrafficProfiler.GetSnapshot(_windowSeconds);

            DrawToolbar(snapshot);
            EditorGUILayout.Space(6f);
            DrawSummary(snapshot);
            EditorGUILayout.Space(8f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawAggregates(snapshot);
            EditorGUILayout.Space(8f);
            DrawRecentSamples(snapshot);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar(NetworkTrafficProfiler.Snapshot snapshot) {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var enabled = GUILayout.Toggle(NetworkTrafficProfiler.Enabled, "Capture", EditorStyles.toolbarButton, GUILayout.Width(72f));
            if (enabled != NetworkTrafficProfiler.Enabled)
                NetworkTrafficProfiler.Enabled = enabled;

            GUILayout.Label("Window", GUILayout.Width(48f));
            _windowSeconds = EditorGUILayout.DoubleField(_windowSeconds, GUILayout.Width(48f));
            _windowSeconds = Math.Max(0.1d, _windowSeconds);
            GUILayout.Label("s", GUILayout.Width(16f));

            GUILayout.FlexibleSpace();
            GUILayout.Label(snapshot.Enabled ? "live" : "paused", EditorStyles.miniLabel, GUILayout.Width(48f));

            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                NetworkTrafficProfiler.Clear();

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawSummary(NetworkTrafficProfiler.Snapshot snapshot) {
            EditorGUILayout.BeginHorizontal();
            DrawMetric("Sent", snapshot.SentBytesInWindow, snapshot.SentPacketsInWindow, snapshot.WindowSeconds);
            DrawMetric("Received", snapshot.ReceivedBytesInWindow, snapshot.ReceivedPacketsInWindow, snapshot.WindowSeconds);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawMetric(string label, int bytes, int packets, double seconds) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(220f));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Bytes", FormatBytes(bytes));
            EditorGUILayout.LabelField("Packets", packets.ToString());
            EditorGUILayout.LabelField("Rate", $"{FormatBytesPerSecond(bytes, seconds)} / {FormatPacketsPerSecond(packets, seconds)}");
            EditorGUILayout.EndVertical();
        }

        private static void DrawAggregates(NetworkTrafficProfiler.Snapshot snapshot) {
            EditorGUILayout.LabelField("By packet type", EditorStyles.boldLabel);
            DrawAggregateHeader();

            foreach (var row in snapshot.Aggregates) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(row.Direction.ToString(), GUILayout.Width(76f));
                EditorGUILayout.LabelField(row.Peer.ToString(), GUILayout.Width(48f));
                EditorGUILayout.LabelField(DeliveryLabel(row.Direction, row.Delivery), GUILayout.Width(140f));
                EditorGUILayout.LabelField(row.PacketType, GUILayout.Width(150f));
                EditorGUILayout.LabelField(row.Packets.ToString(), GUILayout.Width(64f));
                EditorGUILayout.LabelField(FormatBytes(row.Bytes), GUILayout.Width(92f));
                EditorGUILayout.LabelField(row.FailedPackets.ToString(), GUILayout.Width(56f));
                EditorGUILayout.LabelField(FormatLocalTime(row.LastUtcTime), GUILayout.Width(88f));
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawAggregateHeader() {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Dir", EditorStyles.boldLabel, GUILayout.Width(76f));
            EditorGUILayout.LabelField("Peer", EditorStyles.boldLabel, GUILayout.Width(48f));
            EditorGUILayout.LabelField("Delivery", EditorStyles.boldLabel, GUILayout.Width(140f));
            EditorGUILayout.LabelField("Packet", EditorStyles.boldLabel, GUILayout.Width(150f));
            EditorGUILayout.LabelField("Count", EditorStyles.boldLabel, GUILayout.Width(64f));
            EditorGUILayout.LabelField("Bytes", EditorStyles.boldLabel, GUILayout.Width(92f));
            EditorGUILayout.LabelField("Fail", EditorStyles.boldLabel, GUILayout.Width(56f));
            EditorGUILayout.LabelField("Last", EditorStyles.boldLabel, GUILayout.Width(88f));
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawRecentSamples(NetworkTrafficProfiler.Snapshot snapshot) {
            EditorGUILayout.LabelField("Recent packets", EditorStyles.boldLabel);
            DrawSampleHeader();

            foreach (var sample in snapshot.RecentSamples) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(FormatLocalTime(sample.UtcTime), GUILayout.Width(88f));
                EditorGUILayout.LabelField(sample.Direction.ToString(), GUILayout.Width(76f));
                EditorGUILayout.LabelField(sample.Peer.ToString(), GUILayout.Width(48f));
                EditorGUILayout.LabelField(DeliveryLabel(sample.Direction, sample.Delivery), GUILayout.Width(140f));
                EditorGUILayout.LabelField(sample.PacketType, GUILayout.Width(150f));
                EditorGUILayout.LabelField(FormatBytes(sample.Bytes), GUILayout.Width(92f));
                EditorGUILayout.LabelField(sample.Success ? "ok" : "failed", GUILayout.Width(64f));
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawSampleHeader() {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Time", EditorStyles.boldLabel, GUILayout.Width(88f));
            EditorGUILayout.LabelField("Dir", EditorStyles.boldLabel, GUILayout.Width(76f));
            EditorGUILayout.LabelField("Peer", EditorStyles.boldLabel, GUILayout.Width(48f));
            EditorGUILayout.LabelField("Delivery", EditorStyles.boldLabel, GUILayout.Width(140f));
            EditorGUILayout.LabelField("Packet", EditorStyles.boldLabel, GUILayout.Width(150f));
            EditorGUILayout.LabelField("Bytes", EditorStyles.boldLabel, GUILayout.Width(92f));
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(64f));
            EditorGUILayout.EndHorizontal();
        }

        private static string FormatBytes(int bytes) {
            if (bytes < 1024)
                return $"{bytes} B";

            return bytes < 1024 * 1024
                ? $"{bytes / 1024f:0.0} KB"
                : $"{bytes / (1024f * 1024f):0.0} MB";
        }

        private static string FormatBytesPerSecond(int bytes, double seconds) {
            return $"{FormatBytes((int)(bytes / Math.Max(0.1d, seconds)))}/s";
        }

        private static string FormatPacketsPerSecond(int packets, double seconds) {
            return $"{packets / Math.Max(0.1d, seconds):0.0} pkt/s";
        }

        private static string FormatLocalTime(DateTime utcTime) {
            return utcTime.ToLocalTime().ToString("HH:mm:ss.fff");
        }

        private static string DeliveryLabel(NetworkTrafficDirection direction, StaticMlp.Networking.NetDelivery delivery) {
            return direction == NetworkTrafficDirection.Sent ? delivery.ToString() : "-";
        }
    }
}
