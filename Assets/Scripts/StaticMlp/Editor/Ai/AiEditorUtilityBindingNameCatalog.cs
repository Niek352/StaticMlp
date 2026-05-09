using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StaticMlp.Editor.Ai
{
    public static class AiEditorUtilityBindingNameCatalog
    {
        private static Dictionary<ushort, string> _namesByVariableId;

        public static string GetVariableName(ushort variableId)
        {
            _namesByVariableId ??= BuildCache();

            return _namesByVariableId.TryGetValue(variableId, out var name)
                ? name
                : $"var_{variableId}";
        }

        private static Dictionary<ushort, string> BuildCache()
        {
            var result = new Dictionary<ushort, string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < assemblies.Length; i++)
            {
                var types = GetTypesSafe(assemblies[i]);
                foreach (var type in types)
                {
                    if (!IsVariableBindingsType(type))
                        continue;

                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                    for (var j = 0; j < fields.Length; j++)
                    {
                        var field = fields[j];
                        if (field.FieldType != typeof(ushort) || !field.IsLiteral || field.IsInitOnly)
                            continue;

                        var value = (ushort)field.GetRawConstantValue();
                        result.TryAdd(value, FormatFieldName(field.Name));
                    }
                }
            }

            return result;
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

        private static bool IsVariableBindingsType(Type type)
        {
            return type.IsClass
                   && type.IsAbstract
                   && type.IsSealed
                   && type.Name.EndsWith("VariableBindings", StringComparison.Ordinal);
        }

        private static string FormatFieldName(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return fieldName;

            if (fieldName.Contains('_'))
                return fieldName.ToLowerInvariant();

            var chars = new List<char>(fieldName.Length + 8);
            for (var i = 0; i < fieldName.Length; i++)
            {
                var current = fieldName[i];
                if (i > 0 && char.IsUpper(current) && (char.IsLower(fieldName[i - 1]) || char.IsDigit(fieldName[i - 1])))
                    chars.Add('_');

                chars.Add(char.ToLowerInvariant(current));
            }

            return new string(chars.ToArray());
        }
    }
}
