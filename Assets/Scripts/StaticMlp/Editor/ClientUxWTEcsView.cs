using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

public class ClientUxWTEcsView : StaticEcsView<ClientUxWT, ClientUxWTEntityProvider, ClientUxWTEventProvider> {
    [MenuItem("Window/ClientUxWT ECS")]
    public static void OpenWindow() {
        var window = GetWindow<ClientUxWTEcsView>();
        window.Show();
        window.Focus();
    }
}
