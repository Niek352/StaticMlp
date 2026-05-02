using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ClientCoreWTEntityProvider)), CanEditMultipleObjects]
public class ClientCoreWTEntityProviderEditor : StaticEcsEntityProviderEditor<ClientCoreWT, ClientCoreWTEntityProvider> { }
