using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode.Converters
{
    internal static class PlayModeControlsConverters
    {
        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            var stringToVisibility = new ConverterGroup("String field visibility");
            stringToVisibility.AddConverter((ref string value) => string.IsNullOrEmpty(value) ? new StyleEnum<DisplayStyle>(DisplayStyle.None) : new StyleEnum<DisplayStyle>(DisplayStyle.Flex));
            ConverterGroups.RegisterConverterGroup(stringToVisibility);

            var boolToVisibility = new ConverterGroup("Bool field visibility");
            boolToVisibility.AddConverter((ref bool value) => value ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None));
            ConverterGroups.RegisterConverterGroup(boolToVisibility);
        }
    }
}
