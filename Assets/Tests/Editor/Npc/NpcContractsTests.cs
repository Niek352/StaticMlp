using NUnit.Framework;
using StaticMlp.Features.Npc;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcContractsTests
    {
        [Test]
        public void NpcDefinitionId_UsesValueEquality()
        {
            var first = new NpcDefinitionId(7);
            var same = new NpcDefinitionId(7);
            var other = new NpcDefinitionId(8);

            Assert.That(first, Is.EqualTo(same));
            Assert.That(first == same, Is.True);
            Assert.That(first != other, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(same.GetHashCode()));
        }

        [Test]
        public void NpcClass_UsesDesignLockOrdering()
        {
            Assert.That((byte)NpcClass.Companion, Is.EqualTo(1));
            Assert.That((byte)NpcClass.Specialist, Is.EqualTo(2));
        }

        [Test]
        public void NpcAcquisitionPath_UsesDesignLockOrdering()
        {
            Assert.That((byte)NpcAcquisitionPath.Extraction, Is.EqualTo(1));
            Assert.That((byte)NpcAcquisitionPath.Rescue, Is.EqualTo(2));
            Assert.That((byte)NpcAcquisitionPath.Incubation, Is.EqualTo(3));
        }

        [Test]
        public void NpcAcquisitionPathFlags_UseStableBitValues()
        {
            Assert.That((byte)NpcAcquisitionPathFlags.Extraction, Is.EqualTo((byte)(1 << 0)));
            Assert.That((byte)NpcAcquisitionPathFlags.Rescue, Is.EqualTo((byte)(1 << 1)));
            Assert.That((byte)NpcAcquisitionPathFlags.Incubation, Is.EqualTo((byte)(1 << 2)));
            Assert.That((byte)NpcAcquisitionPathFlags.Seeded, Is.EqualTo((byte)(1 << 3)));
        }

        [Test]
        public void NpcRoleFlags_UseStableBitValues()
        {
            Assert.That((ushort)NpcRoleFlags.Gatherer, Is.EqualTo((ushort)(1 << 0)));
            Assert.That((ushort)NpcRoleFlags.Hauler, Is.EqualTo((ushort)(1 << 1)));
            Assert.That((ushort)NpcRoleFlags.Processor, Is.EqualTo((ushort)(1 << 2)));
            Assert.That((ushort)NpcRoleFlags.Guard, Is.EqualTo((ushort)(1 << 3)));
            Assert.That((ushort)NpcRoleFlags.Builder, Is.EqualTo((ushort)(1 << 4)));
            Assert.That((ushort)NpcRoleFlags.Researcher, Is.EqualTo((ushort)(1 << 5)));
        }
    }
}
