using FFS.Libraries.StaticEcs.Unity.Editor;
using StaticMlp.Networking;
using UnityEditor;

[CustomEditor(typeof(ClientUxWTEventProvider)), CanEditMultipleObjects]
public class ClientUxWTEventProviderEditor : StaticEcsEvenTEntityProviderEditor<ClientUxWT, ClientUxWTEventProvider> { }
