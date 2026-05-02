using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ServerWTEntityProvider)), CanEditMultipleObjects]
public class ServerWTEntityProviderEditor : StaticEcsEntityProviderEditor<ServerWT, ServerWTEntityProvider> { }
