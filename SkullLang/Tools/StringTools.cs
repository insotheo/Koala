using System;
using System.Text.RegularExpressions;

namespace SkullLang.Tools
{
    internal static class StringTools
    {
        static string _toUpperSnakeCasePattern = @"(?<=[a-z0-9])([A-Z])|(?<=[A-Z])([A-Z])(?=[a-z])";

        internal static string ToUpperSnakeCase(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return input;

            string result = Regex.Replace(input, _toUpperSnakeCasePattern, "_$1$2");

            result = Regex.Replace(result, @"__+", "_");

            return result.ToUpper();
        }
    }
}
