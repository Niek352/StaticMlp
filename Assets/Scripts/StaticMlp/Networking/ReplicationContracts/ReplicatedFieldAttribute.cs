using System;

namespace StaticMlp.Networking.Replication {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReplicatedFieldAttribute : Attribute {
        public float Quantize;
        public bool Compress;
    }
}
