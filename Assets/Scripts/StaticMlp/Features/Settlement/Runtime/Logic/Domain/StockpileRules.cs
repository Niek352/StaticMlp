using System;

namespace StaticMlp.Features.Settlement
{
    /// <summary>
    /// Pure domain rules for stockpile capacity contribution and settlement storage acceptance.
    /// </summary>
    public static class StockpileRules
    {
        /// <summary>
        /// Clamps the requested add amount to the remaining capacity in settlement storage.
        /// Returns the amount that can actually be accepted (0 if at or over capacity).
        /// </summary>
        public static int ClampToCapacity(int capacity, int totalUsed, int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var remaining = Math.Max(0, capacity - totalUsed);
            return Math.Min(requestedAmount, remaining);
        }

        /// <summary>
        /// Returns true when the settlement storage has room for at least one more unit.
        /// </summary>
        public static bool HasAvailableCapacity(int capacity, int totalUsed)
        {
            return totalUsed < capacity;
        }

        /// <summary>
        /// Returns the new total capacity after adding a stockpile building's contribution.
        /// </summary>
        public static int AddCapacityContribution(int currentCapacity, ushort contribution)
        {
            return currentCapacity + contribution;
        }
    }
}
