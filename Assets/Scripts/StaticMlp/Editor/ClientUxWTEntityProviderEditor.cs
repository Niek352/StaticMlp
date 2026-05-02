using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ClientUxWTEntityProvider)), CanEditMultipleObjects]
public class ClientUxWTEntityProviderEditor : StaticEcsEntityProviderEditor<ClientUxWT, ClientUxWTEntityProvider> { }
