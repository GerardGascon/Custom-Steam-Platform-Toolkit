using System;
using System.Collections.Generic;

namespace Unity.PlatformToolkit.PlayMode
{
    internal static class EnumerableExtension
    {
        /// <summary>
        /// Assign a unique string for each value. Done by adding a suffix to repeating values.
        /// Useful when displaying a list of strings where all of them must be distinct, but the source values can repeat.
        /// </summary>
        /// <param name="values">Source values, for which unique strings are generated.</param>
        /// <param name="stringGetter">Function for transforming values to strings.</param>
        /// <returns>A tuple consisting of the original value and its assigned unique string.</returns>
        public static IEnumerable<(string UniqueString, T Value)> AssignUniqueString<T>(this IEnumerable<T> values, Func<T, string> stringGetter)
        {
            var list = new List<(string, T)>();
            var titles = new HashSet<string>();
            foreach (var value in values)
            {
                var title = stringGetter(value);
                if (titles.Add(title))
                    list.Add((title, value));
                else
                {
                    var index = 1;
                    var titleWithSuffix = $"{title} ({index})";
                    while (!titles.Add(titleWithSuffix))
                    {
                        index++;
                        titleWithSuffix = $"{title} ({index})";
                    }
                    list.Add((titleWithSuffix, value));
                }
            }
            return list;
        }
    }
}
