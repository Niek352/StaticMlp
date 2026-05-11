using System;
using System.Globalization;
using UnityEditor;

namespace UnityPrefabXML
{
    internal static class SerializedPropertyEnumSupport
    {
        public static void SetEnumValue(SerializedProperty property, string value)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var enumInt))
            {
                property.intValue = enumInt;
                return;
            }

            if (TryGetExactEnumType(property, out var enumType))
            {
                property.intValue = Convert.ToInt32(Enum.Parse(enumType, value, true), CultureInfo.InvariantCulture);
                return;
            }

            var enumIndex = FindEnumIndex(property, value);
            if (enumIndex >= 0)
            {
                property.enumValueIndex = enumIndex;
                return;
            }
            throw new FormatException(
                $"Cannot resolve enum value '{value}' for property '{property.propertyPath}'. Available values: {string.Join(", ", property.enumNames)}");
        }

        public static string GetEnumValue(SerializedProperty property)
        {
            if (TryGetExactEnumType(property, out var enumType))
                return Enum.ToObject(enumType, property.intValue).ToString();

            var enumIndex = property.enumValueIndex;
            if (enumIndex >= 0 && enumIndex < property.enumNames.Length)
                return property.enumNames[enumIndex];

            return property.intValue.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryGetExactEnumType(SerializedProperty property, out Type enumType)
        {
            var fieldInfo = ScriptAttributeUtilityProxy.GetFieldInfoAndStaticTypeFromProperty(property, out var staticType);
            if (fieldInfo != null && fieldInfo.FieldType.IsEnum)
            {
                enumType = fieldInfo.FieldType;
                return true;
            }

            if (staticType != null && staticType.IsEnum)
            {
                enumType = staticType;
                return true;
            }

            enumType = null;
            return false;
        }

        private static int FindEnumIndex(SerializedProperty property, string value)
        {
            var normalizedInput = NormalizeEnumToken(value);

            for (var i = 0; i < property.enumNames.Length; i++)
            {
                if (string.Equals(property.enumNames[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;

                if (i < property.enumDisplayNames.Length
                    && string.Equals(property.enumDisplayNames[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;

                if (string.Equals(NormalizeEnumToken(property.enumNames[i]), normalizedInput, StringComparison.Ordinal))
                    return i;

                if (i < property.enumDisplayNames.Length
                    && string.Equals(NormalizeEnumToken(property.enumDisplayNames[i]), normalizedInput, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }

        private static string NormalizeEnumToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var buffer = new char[value.Length];
            var count = 0;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!char.IsLetterOrDigit(c))
                    continue;

                buffer[count++] = char.ToLowerInvariant(c);
            }

            return new string(buffer, 0, count);
        }
    }
}
