using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class AchievementCommonConfiguration
    {
        public static string EmptyAchievementFound = "Empty achievement ID found.";
        private readonly VisualTreeAsset m_CellVisualTreeAsset;

        private readonly ObservableSerializableList<StoredAchievement> m_StoredAchievements;

        private Dictionary<StoredAchievement, CommonCellViewModel> m_ViewModels =
            new Dictionary<StoredAchievement, CommonCellViewModel>();

        public HeaderViewModel m_HeaderViewModel { get; private set; }

        private readonly Texture2D m_WarningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;

        public AchievementCommonConfiguration(ObservableSerializableList<StoredAchievement> achievements)
        {
            m_CellVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.unity.platformtoolkit/Editor/Achievement/UI/AchievementEditorCommonCell.uxml");
            m_StoredAchievements = achievements;

            m_HeaderViewModel = new HeaderViewModel();

            var m_StoredAchievementObserver = new StoredAchievementObserver(m_StoredAchievements);
            m_StoredAchievementObserver.AchievementsChanged += StoredAchievementObserverOnAchievementsChanged;

            StoredAchievementObserverOnAchievementsChanged();
        }

        private void StoredAchievementObserverOnAchievementsChanged()
        {
            HashSet<string> allWarnings = new HashSet<string>();
            var duplicates = FindDuplicates();
            foreach (var achievement in m_StoredAchievements)
            {
                var id = achievement.Id;
                var cell = GetCellViewModel(achievement);
                cell.Warnings = Array.Empty<string>();

                if (string.IsNullOrEmpty(achievement.Id))
                {
                    allWarnings.Add(EmptyAchievementFound);
                    cell.Warnings = new[] { EmptyAchievementFound };
                }

                if (duplicates.Contains(id))
                {
                    string warning = CreateDuplicateIdFoundWarning(achievement.Id);
                    allWarnings.Add(warning);
                    cell.Warnings = new[] { warning };
                }
            }
            m_HeaderViewModel.Warnings = allWarnings.ToArray();
        }

        public string CreateDuplicateIdFoundWarning(string duplicateId)
        {
            return $"Duplicate ID found: {duplicateId}.";
        }

        private HashSet<string> FindDuplicates()
        {
            var achievementIds = new HashSet<string>();
            var duplicates = new HashSet<string>();
            foreach (var achievement in m_StoredAchievements)
            {
                if (string.IsNullOrEmpty(achievement.Id))
                    continue;

                if (!achievementIds.Add(achievement.Id))
                {
                    duplicates.Add(achievement.Id);
                }
            }

            return duplicates;
        }

        public CommonCellViewModel GetCellViewModel(StoredAchievement achievement)
        {
            if (m_ViewModels.TryGetValue(achievement, out var viewModel))
                return viewModel;

            viewModel = new CommonCellViewModel() { StoredAchievement = achievement };
            m_ViewModels.Add(achievement, viewModel);
            return viewModel;
        }

        public VisualElement MakeHeader()
        {
            var container = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.unity.platformtoolkit/Editor/Achievement/UI/AchievementEditorCommonHeader.uxml")
                .Instantiate();
            container.dataSource = m_HeaderViewModel;
            var iconVisualElement = container.Q<VisualElement>("warning-icon");
            iconVisualElement.style.backgroundImage = new StyleBackground(m_WarningIcon);
            return container;
        }

        public VisualElement MakeCell()
        {
            var cell = m_CellVisualTreeAsset.Instantiate().Children().First();
            var iconVisualElement = cell.Q<VisualElement>("warning-icon");
            iconVisualElement.style.backgroundImage = new StyleBackground(m_WarningIcon);
            return cell;
        }

        public void BindCell(VisualElement cell, StoredAchievement achievement)
        {
            cell.dataSource = GetCellViewModel(achievement);
        }

        public void UnbindCell(VisualElement cell)
        {
            cell.dataSource = null;
        }
    }
}
