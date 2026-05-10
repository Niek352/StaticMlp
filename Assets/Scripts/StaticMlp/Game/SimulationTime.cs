using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Game
{
    public sealed class SimulationTime : IResource
    {
        public uint ServerTick;
        public float FixedStepSeconds;
        public double ElapsedSeconds;

        public uint SecondsToTicks(float seconds)
        {
            if (seconds <= 0f)
                return 0;

            if (FixedStepSeconds <= 0f)
                throw new InvalidOperationException("SimulationTime.FixedStepSeconds must be greater than zero.");

            return (uint)Math.Ceiling(seconds / FixedStepSeconds);
        }

        public uint DeadlineAfter(float seconds)
        {
            return ServerTick + SecondsToTicks(seconds);
        }

        public void AdvanceTick()
        {
            ServerTick++;
            ElapsedSeconds += FixedStepSeconds;
        }
    }
}
