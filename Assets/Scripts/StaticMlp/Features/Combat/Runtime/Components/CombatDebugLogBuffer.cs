using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public sealed class CombatDebugLogBuffer : IResource
    {
        private const int MAX_ENTRIES = 128;
        private readonly List<string> _entries = new(MAX_ENTRIES);

        public IReadOnlyList<string> Entries => _entries;

        public void Append(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Combat debug log message must not be null or empty.", nameof(message));

            if (_entries.Count == MAX_ENTRIES)
                _entries.RemoveAt(0);

            _entries.Add(message);
        }
    }
}
