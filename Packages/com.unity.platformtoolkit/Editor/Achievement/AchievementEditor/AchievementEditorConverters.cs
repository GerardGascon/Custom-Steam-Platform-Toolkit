using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal static class AchievementEditorConverters
    {
        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            var progressTypeConverters = new ConverterGroup("Unlock Type Converters");
            progressTypeConverters.AddConverter((ref UnlockType unlockType) =>
            {
                return unlockType switch
                {
                    UnlockType.Single => new StyleEnum<DisplayStyle>(DisplayStyle.None),
                    UnlockType.Progressive => new StyleEnum<DisplayStyle>(DisplayStyle.Flex),
                    _ => throw new ArgumentOutOfRangeException(nameof(unlockType), unlockType, null)
                };
            });

            ConverterGroups.RegisterConverterGroup(progressTypeConverters);

            var warningConverter = new ConverterGroup("Warning Converters");
            warningConverter.AddConverter((ref IReadOnlyList<string> warnings) => warnings == null || warnings.Count == 0 ? string.Empty : string.Join('\n', warnings));

            warningConverter.AddConverter((ref IReadOnlyList<string> warnings) =>
            {
                if (warnings == null || warnings.Count == 0)
                {
                    return new StyleEnum<DisplayStyle>(DisplayStyle.None);
                }
                else
                {
                    return new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }
            });
            ConverterGroups.RegisterConverterGroup(warningConverter);

            var warningTextConverter = new ConverterGroup("StringIdCellViewModel Converter");
            warningTextConverter.AddConverter((ref StringIdCellViewModel stringIdCell) => stringIdCell.WarningViewModel.Warnings == null || stringIdCell.WarningViewModel.Warnings.Count == 0 ? string.Empty : string.Join('\n', stringIdCell.WarningViewModel.Warnings));

            warningTextConverter.AddConverter((ref StringIdCellViewModel stringIdCell) =>
            {
                if (stringIdCell.WarningViewModel.Warnings == null || stringIdCell.WarningViewModel.Warnings.Count == 0 || stringIdCell.WarningViewModel.Ignored)
                {
                    return new StyleEnum<DisplayStyle>(DisplayStyle.None);
                }
                else
                {
                    return new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }
            });
            ConverterGroups.RegisterConverterGroup(warningTextConverter);

            var boolConverter = new ConverterGroup("Backwards bool converter");
            boolConverter.AddConverter((ref bool stringIdCell) => !stringIdCell);
            ConverterGroups.RegisterConverterGroup(boolConverter);
        }
    }
}
