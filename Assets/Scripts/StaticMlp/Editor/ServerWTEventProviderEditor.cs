using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ServerWTEventProvider)), CanEditMultipleObjects]
public class ServerWTEventProviderEditor : StaticEcsEvenTEntityProviderEditor<ServerWT, ServerWTEventProvider> { }
