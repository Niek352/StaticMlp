using System;

namespace StaticMlp.Networking.Replication {
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class NetworkEntityManifestAttribute : Attribute {
        public Type[] ReplicatedComponents { get; }

        public NetworkEntityManifestAttribute(params Type[] replicatedComponents) {
            ReplicatedComponents = replicatedComponents ?? Array.Empty<Type>();
        }
    }
}
