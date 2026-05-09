using System;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiBlackboardFloatBinding
    {
        private readonly Func<SW.Entity, float> _reader;

        public AiBlackboardFloatBinding(ushort variableId, Func<SW.Entity, float> reader)
        {
            VariableId = variableId;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public ushort VariableId { get; }

        public float Read(SW.Entity entity)
        {
            return _reader(entity);
        }
    }
}
