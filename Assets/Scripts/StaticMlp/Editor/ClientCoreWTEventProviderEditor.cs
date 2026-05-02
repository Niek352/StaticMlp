using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ClientCoreWTEventProvider)), CanEditMultipleObjects]
public class ClientCoreWTEventProviderEditor : StaticEcsEvenTEntityProviderEditor<ClientCoreWT, ClientCoreWTEventProvider> { }
