using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class StringIdCellViewModel
    {
        [CreateProperty]
        public IAchievement Achievement { get; }

        [CreateProperty]
        public WarningViewModel WarningViewModel { get; }

        [CreateProperty]
        public string Id
        {
            get => Achievement.ImplementationData.ConfigurationData;
            set
            {
                var trimmedValue = value.Trim();
                if (trimmedValue.Length > AchievementEditor.AchievementCharacterLimit)
                {
                    Debug.LogWarning($"Character limit exceeded in achievement ID. Limit: {AchievementEditor.AchievementCharacterLimit}.");
                    return;
                }
                if (!Regex.IsMatch(trimmedValue, AchievementEditor.AchievementImplementationDataRegexPattern))
                {
                    Achievement.ImplementationData.ConfigurationData = trimmedValue;
                }
                else
                {
                    Debug.LogWarning("No spaces allowed in achievement ID.");
                }
            }
        }

        public StringIdCellViewModel(IAchievement achievement)
        {
            Achievement = achievement;
            WarningViewModel = new WarningViewModel(achievement);
        }
    }

    internal class WarningViewModel
    {
        [CreateProperty]
        public bool Ignored => m_Achievement.ImplementationData.Ignore;

        [CreateProperty]
        public IReadOnlyList<string> Warnings { get; set; }

        private IAchievement m_Achievement;

        public WarningViewModel(IAchievement achievement)
        {
            m_Achievement = achievement;
        }
    }
}
