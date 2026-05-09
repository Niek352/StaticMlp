using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public sealed class AiActionCatalog : IResource
    {
        private readonly AiBehaviorCatalog _behaviorCatalog;
        private readonly IAiTaskExecutor _fallbackExecutor;
        private readonly Dictionary<AiTaskType, IAiTaskExecutor> _executors;
        private readonly Dictionary<AiTaskType, IAiActionCommandTargetBinder> _manualCommandBinders;
        private readonly Dictionary<ushort, AiBlackboardFloatBinding> _utilityBindings;
        private readonly IAiActionVariableCollector[] _variableCollectors;

        private AiActionCatalog(
            AiBehaviorCatalog behaviorCatalog,
            IAiTaskExecutor fallbackExecutor,
            Dictionary<AiTaskType, IAiTaskExecutor> executors,
            Dictionary<AiTaskType, IAiActionCommandTargetBinder> manualCommandBinders,
            Dictionary<ushort, AiBlackboardFloatBinding> utilityBindings,
            IAiActionVariableCollector[] variableCollectors)
        {
            _behaviorCatalog = behaviorCatalog ?? throw new ArgumentNullException(nameof(behaviorCatalog));
            _fallbackExecutor = fallbackExecutor ?? throw new ArgumentNullException(nameof(fallbackExecutor));
            _executors = executors ?? throw new ArgumentNullException(nameof(executors));
            _manualCommandBinders = manualCommandBinders ?? throw new ArgumentNullException(nameof(manualCommandBinders));
            _utilityBindings = utilityBindings ?? throw new ArgumentNullException(nameof(utilityBindings));
            _variableCollectors = variableCollectors ?? Array.Empty<IAiActionVariableCollector>();
        }

        public static AiActionCatalog Discover(AiTaskExecutionTransitions transitions)
        {
            if (transitions == null)
                throw new ArgumentNullException(nameof(transitions));

            var packages = DiscoverPackages();
            var utilityBindings = new Dictionary<ushort, AiBlackboardFloatBinding>();
            var manualCommandBinders = new Dictionary<AiTaskType, IAiActionCommandTargetBinder>();
            var executors = new Dictionary<AiTaskType, IAiTaskExecutor>();
            var collectors = new List<IAiActionVariableCollector>();
            var tasksByBehavior = new Dictionary<ushort, List<UtilityTaskDefinition>>();

            for (var i = 0; i < packages.Length; i++)
            {
                var package = packages[i];
                RegisterExecutor(package, transitions, executors);
                RegisterManualCommandBinder(package, manualCommandBinders);
                RegisterUtilityBindings(package, utilityBindings);
                RegisterBehaviorContributions(package, tasksByBehavior);
                if (package.VariableCollector != null)
                    collectors.Add(package.VariableCollector);
            }

            if (!executors.TryGetValue(AiTaskType.Idle, out var idleExecutor))
            {
                throw new InvalidOperationException(
                    $"AI action discovery must register an executor for task '{AiTaskType.Idle}'.");
            }

            var behaviors = tasksByBehavior
                .OrderBy(pair => pair.Key)
                .Select(pair => new AiBehaviorDefinition
                {
                    BehaviorId = pair.Key,
                    Tasks = pair.Value.ToArray()
                })
                .ToArray();

            return new AiActionCatalog(
                new AiBehaviorCatalog(behaviors),
                idleExecutor,
                executors,
                manualCommandBinders,
                utilityBindings,
                collectors.ToArray());
        }

        public bool TryGetBehavior(ushort behaviorId, out AiBehaviorDefinition behavior)
        {
            return _behaviorCatalog.TryGetBehavior(behaviorId, out behavior);
        }

        public IAiTaskExecutor ResolveExecutor(AiTaskType taskType)
        {
            return _executors.GetValueOrDefault(taskType, _fallbackExecutor);
        }

        public bool SupportsManualCommand(AiTaskType taskType)
        {
            return _manualCommandBinders.ContainsKey(taskType);
        }

        public bool TryBindManualCommand(AiTaskType taskType, SW.Entity bot, EntityGID target)
        {
            return _manualCommandBinders.TryGetValue(taskType, out var binder)
                   && binder.TryBind(bot, target);
        }

        public void CollectVariables(SW.Entity entity)
        {
            for (var i = 0; i < _variableCollectors.Length; i++)
                _variableCollectors[i].Collect(entity);
        }

        public float ReadUtilityValue(SW.Entity entity, ushort variableId)
        {
            return _utilityBindings.TryGetValue(variableId, out var binding)
                ? binding.Read(entity)
                : 0f;
        }

        private static IAiActionPackage[] DiscoverPackages()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetTypesSafe)
                .Where(IsPackageType)
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .Select(type => (IAiActionPackage)Activator.CreateInstance(type))
                .ToArray();
        }

        private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(type => type != null).Cast<Type>();
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsPackageType(Type type)
        {
            return typeof(IAiActionPackage).IsAssignableFrom(type)
                   && !type.IsAbstract
                   && !type.IsInterface
                   && type.GetConstructor(Type.EmptyTypes) != null;
        }

        private static void RegisterExecutor(
            IAiActionPackage package,
            AiTaskExecutionTransitions transitions,
            Dictionary<AiTaskType, IAiTaskExecutor> executors)
        {
            var executor = package.CreateExecutor(transitions)
                           ?? throw new InvalidOperationException(
                               $"AI action package '{package.GetType().FullName}' returned a null executor.");

            if (executor.TaskType != package.TaskType)
            {
                throw new InvalidOperationException(
                    $"AI action package '{package.GetType().FullName}' created executor '{executor.TaskType}' for declared task '{package.TaskType}'.");
            }

            if (executors.ContainsKey(package.TaskType))
            {
                throw new InvalidOperationException(
                    $"AI task executor for task '{package.TaskType}' is already registered.");
            }

            executors.Add(package.TaskType, executor);
        }

        private static void RegisterManualCommandBinder(
            IAiActionPackage package,
            Dictionary<AiTaskType, IAiActionCommandTargetBinder> manualCommandBinders)
        {
            if (package.ManualCommandTargetBinder == null)
                return;

            if (manualCommandBinders.ContainsKey(package.TaskType))
            {
                throw new InvalidOperationException(
                    $"AI command target binder for task '{package.TaskType}' is already registered.");
            }

            manualCommandBinders.Add(package.TaskType, package.ManualCommandTargetBinder);
        }

        private static void RegisterUtilityBindings(
            IAiActionPackage package,
            Dictionary<ushort, AiBlackboardFloatBinding> utilityBindings)
        {
            var bindings = package.UtilityBindings;
            if (bindings == null)
                return;

            for (var i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null)
                {
                    throw new InvalidOperationException(
                        $"AI action package '{package.GetType().FullName}' contains a null utility binding.");
                }

                if (!utilityBindings.TryAdd(binding.VariableId, binding))
                {
                    throw new InvalidOperationException(
                        $"AI utility binding variable '{binding.VariableId}' is already registered.");
                }
            }
        }

        private static void RegisterBehaviorContributions(
            IAiActionPackage package,
            Dictionary<ushort, List<UtilityTaskDefinition>> tasksByBehavior)
        {
            var contributions = package.UtilityTaskContributions;
            if (contributions == null)
                return;

            for (var i = 0; i < contributions.Count; i++)
            {
                var contribution = contributions[i];
                if (!tasksByBehavior.TryGetValue(contribution.BehaviorId, out var tasks))
                {
                    tasks = new List<UtilityTaskDefinition>();
                    tasksByBehavior.Add(contribution.BehaviorId, tasks);
                }

                tasks.Add(new UtilityTaskDefinition
                {
                    Task = package.TaskType,
                    Considerations = contribution.Considerations ?? Array.Empty<UtilityConsideration>()
                });
            }
        }
    }
}
