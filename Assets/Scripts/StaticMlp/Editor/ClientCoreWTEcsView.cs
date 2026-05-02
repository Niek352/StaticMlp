using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

public class ClientCoreWTEcsView : StaticEcsView<ClientCoreWT, ClientCoreWTEntityProvider, ClientCoreWTEventProvider> {
    [MenuItem("Window/ClientCoreWT ECS")]
    public static void OpenWindow() {
        var window = GetWindow<ClientCoreWTEcsView>();
        window.Show();
        window.Focus();
    }
}
