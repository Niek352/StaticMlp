using System;
using System.Collections.Generic;
using System.Linq;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiBehaviorTaskRegistry
    {
        private readonly Dictionary<ushort, List<UtilityTaskDefinition>> _tasksByBehavior = new();

        public void Add(ushort behaviorId, AiTaskType taskType, UtilityConsideration[] considerations)
        {
            if (!_tasksByBehavior.TryGetValue(behaviorId, out var tasks))
            {
                tasks = new List<UtilityTaskDefinition>();
                _tasksByBehavior.Add(behaviorId, tasks);
            }

            tasks.Add(new UtilityTaskDefinition
            {
                Task = taskType,
                Considerations = considerations ?? Array.Empty<UtilityConsideration>()
            });
        }

        public AiBehaviorDefinition[] BuildDefinitions()
        {
            return _tasksByBehavior
                .OrderBy(pair => pair.Key)
                .Select(pair => new AiBehaviorDefinition
                {
                    BehaviorId = pair.Key,
                    Tasks = pair.Value.ToArray()
                })
                .ToArray();
        }
    }
}
