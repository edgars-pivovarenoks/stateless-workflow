using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Stateless.Workflow
{
    internal static class HelperExtensions
    {
        internal static string With(this string value, params object[] args)
        {
            return string.Format(value, args);
        }
        internal static ICollection<T> ThrowIfNone<T>(this ICollection<T> listOfParams, string paramName)
        {
            if (!listOfParams.Any())
                throw new ArgumentException(
                    $"List of parameters '{paramName}' should not be empty and should contain at least one element.");

            return
                listOfParams;
        }

        internal static T ThrowIfNull<T>(this T o, string paramName)
        {
            if (o == null)
                throw new ArgumentNullException(paramName);

            return
                o;
        }

        internal static string ToSeparatedString<T>(this IEnumerable<T> array, string separator = ",")
        {
            return string.Join(separator, array);
        }
        internal static string GetDescription(this Type type)
        {
            var descriptions = (DescriptionAttribute[])
                type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descriptions.Length == 0)
            {
                return null;
            }
            return descriptions[0].Description;
        }
        internal static string SplitCamelCase(this string input)
        {
            MatchEvaluator replaceMatch = m =>
            {
                string result;

                if (m.Groups.Count == 3)
                    result = m.Groups[2].Value.IsUpper() ? m.Value : m.Value.ToLower();
                else
                    result = m.Value.ToLower();

                return $" {result}";
            };

            return Regex
                .Replace(input, "(?<=[a-z])([A-Z])(?=([a-z,A-Z]))", replaceMatch);
        }
        internal static bool IsUpper(this string value)
        {
            return value
                .All(t => !char.IsLower(t));
        }
    }
}
