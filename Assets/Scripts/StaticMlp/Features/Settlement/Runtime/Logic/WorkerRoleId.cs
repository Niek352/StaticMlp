using System;

namespace StaticMlp.Features.Settlement
{
    public readonly struct WorkerRoleId : IEquatable<WorkerRoleId>
    {
        public readonly ushort Value;

        public WorkerRoleId(ushort value)
        {
            Value = value;
        }

        public bool Equals(WorkerRoleId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is WorkerRoleId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(WorkerRoleId left, WorkerRoleId right) => left.Equals(right);
        public static bool operator !=(WorkerRoleId left, WorkerRoleId right) => !left.Equals(right);
    }
}
