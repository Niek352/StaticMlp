using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

public class ServerWTEcsView : StaticEcsView<ServerWT, ServerWTEntityProvider, ServerWTEventProvider> {
    [MenuItem("Window/ServerWT ECS")]
    public static void OpenWindow() {
        var window = GetWindow<ServerWTEcsView>();
        window.Show();
        window.Focus();
    }
}
