using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal class AchievementConfigurationWithStringId : IAchievementConfiguration, IAchievementEditorProvider, IAchievementImportExport
    {
        public static readonly string EmptyAchievementFound = "Empty achievement ID found.";
        private IAchievementConfigurationContext m_Context;
        private readonly VisualTreeAsset m_CellVisualTreeAsset;
        private readonly VisualTreeAsset m_HeaderVisualTreeAsset;
        private readonly VisualElement m_WarningIconHeader;

        private Dictionary<IAchievement, StringIdCellViewModel> m_ViewModels =
            new Dictionary<IAchievement, StringIdCellViewModel>();

        public HeaderViewModel HeaderViewModel { get; private set; }

        public IAchievementEditorProvider EditorProvider => this;

        private readonly Texture2D m_WarningIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
        private readonly string m_Title;
        public IAchievementImportExport ImportExportProvider => this;
        public string PlatformName => m_Title;

        public AchievementConfigurationWithStringId(IAchievementConfigurationContext context, string title, string exportKey)
        {
            m_Context = context;
            m_CellVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.unity.platformtoolkit/Editor/Achievement/UI/AchievementEditorStringIdCell.uxml");
            m_HeaderVisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.unity.platformtoolkit/Editor/Achievement/UI/AchievementEditorStringIdHeader.uxml");
            m_Title = title;
            HeaderViewModel = new HeaderViewModel() { HeaderText = m_Title };
            ExportKey = exportKey;
            var achievementObserver = new AchievementContextObserver(m_Context);
            achievementObserver.AchievementConfigurationDataChanged += RefreshWarnings;
            m_Context.AchievementRemoved += OnAchievementRemoved;
            m_Context.AchievementAdded += OnAchievementAdded;
            RefreshWarnings();
        }

        private void OnAchievementAdded(IAchievement achievement)
        {
            m_ViewModels.Add(achievement, new StringIdCellViewModel(achievement));
            RefreshWarnings();
        }

        private void OnAchievementRemoved(IAchievement removedAchievement)
        {
            m_ViewModels?.Remove(removedAchievement);
            RefreshWarnings();
        }

        public string ExportKey { get; }

        public void Import(string exportedConfigurationData, IAchievement achievement)
        {
            achievement.ImplementationData.ConfigurationData = exportedConfigurationData == AchievementEditor.AchievementIgnoreTag ? "" : exportedConfigurationData;
            achievement.ImplementationData.Ignore = exportedConfigurationData == AchievementEditor.AchievementIgnoreTag;
        }

        public ImplementationData Export(IAchievement achievement)
        {
            return achievement.ImplementationData;
        }

        private string CreateDuplicateIdFoundWarning(string duplicateId)
        {
            return $"Duplicate ID found: {duplicateId}.";
        }

        private HashSet<string> FindDuplicates()
        {
            var achievementIds = new HashSet<string>();
            var duplicates = new HashSet<string>();
            foreach (var achievement in m_Context.Achievements)
            {
                if(achievement.ImplementationData.Ignore)
                    continue;
                if (string.IsNullOrEmpty(achievement.ImplementationData.ConfigurationData))
                    continue;

                if (!achievementIds.Add(achievement.ImplementationData.ConfigurationData))
                {
                    duplicates.Add(achievement.ImplementationData.ConfigurationData);
                }
            }

            return duplicates;
        }

        public static AchievementDefinitionWithNativeId<string>[] BuildForRuntime(IAchievementConfigurationContext achievementContext)
        {
            return achievementContext.Achievements
                .Where(a => !string.IsNullOrEmpty(a.Id) && !string.IsNullOrEmpty(a.ImplementationData.ConfigurationData) && !a.ImplementationData.Ignore)
                .Select(a => new AchievementDefinitionWithNativeId<string>(
                    a.Id,
                    a.ImplementationData.ConfigurationData,
                    a.UnlockType == UnlockType.Progressive,
                    a.ProgressTarget
                    ))
                .ToArray();
        }

        public static AchievementDefinitionWithNativeId<int>[] BuildForRuntimeWithIntIds(IAchievementConfigurationContext achievementContext)
        {
            var achievementList = new List<AchievementDefinitionWithNativeId<int>>();
            foreach (var achievement in achievementContext.Achievements)
            {
                if (achievement.ImplementationData.Ignore)
                    continue;

                if (string.IsNullOrEmpty(achievement.Id))
                {
                    Debug.LogWarning("Found an achievement with an empty Id. The achievement will be excluded from the build.");
                    continue;
                }

                if (string.IsNullOrEmpty(achievement.ImplementationData.ConfigurationData))
                {
                    Debug.LogWarning($"Achievement {achievement.Id} has an empty platform Id. The achievement will be excluded from the build.");
                    continue;
                }

                if (int.TryParse(achievement.ImplementationData.ConfigurationData, out var intNativeId))
                {
                    achievementList.Add(new AchievementDefinitionWithNativeId<int>(
                        achievement.Id,
                        intNativeId,
                        achievement.UnlockType == UnlockType.Progressive,
                        achievement.ProgressTarget
                    ));
                }
                else
                {
                    throw new InvalidOperationException($"Could not parse a platform achievement ID for achievement {achievement.Id} an integer expected, but was {achievement.ImplementationData.ConfigurationData}.");
                }
            }
            return achievementList.ToArray();
        }

        private IEnumerable<IAchievement> FindWithDuplicateNativeId()
        {
            return m_Context.Achievements.GroupBy(a => a.ImplementationData.ConfigurationData).Where(g => g.Count() > 1)
                .Select(g => g.First());
        }

        public StringIdCellViewModel GetCellViewModel(IAchievement achievement)
        {
            if (m_ViewModels.TryGetValue(achievement, out var viewModel))
                return viewModel;

            viewModel = new StringIdCellViewModel(achievement);

            m_ViewModels.Add(achievement, viewModel);
            return viewModel;
        }

        public VisualElement MakeHeader()
        {
            // Skipping the template element, which is automatically created when calling Instantiate
            var header = m_HeaderVisualTreeAsset.Instantiate().Children().First();
            var iconVisualElement = header.Q<VisualElement>("warning-icon");
            iconVisualElement.style.backgroundImage = new StyleBackground(m_WarningIcon);
            header.dataSource = HeaderViewModel;
            return header;
        }

        public VisualElement MakeCell()
        {
            // Skipping the template element, which is automatically created when calling Instantiate
            var cell = m_CellVisualTreeAsset.Instantiate().Children().First();
            var iconVisualElement = cell.Q<VisualElement>("warning-icon");
            iconVisualElement.style.backgroundImage = new StyleBackground(m_WarningIcon);
            return cell;
        }

        public void BindCell(VisualElement cell, IAchievement achievement)
        {
            cell.dataSource = GetCellViewModel(achievement);
            var dropdown = cell.Q<AchievementIgnoreDropdown>();
            dropdown.Achievement = achievement;
        }

        public void UnbindCell(VisualElement cell)
        {
            cell.dataSource = null;
        }

        private void RefreshWarnings()
        {
            HashSet<string> localWarnings = new HashSet<string>();
            HeaderViewModel.Warnings = Array.Empty<string>();
            var duplicates = FindDuplicates();

            foreach (var achievement in m_Context.Achievements)
            {
                if(achievement.ImplementationData.Ignore)
                    continue;
                var id = achievement.ImplementationData.ConfigurationData;
                var cell = GetCellViewModel(achievement);
                cell.WarningViewModel.Warnings = Array.Empty<string>();

                if (string.IsNullOrEmpty(achievement.ImplementationData.ConfigurationData))
                {
                    cell.WarningViewModel.Warnings = new[] { EmptyAchievementFound };
                    localWarnings.Add(EmptyAchievementFound);
                }

                if (duplicates.Contains(id))
                {
                    string warning = CreateDuplicateIdFoundWarning(achievement.ImplementationData.ConfigurationData);
                    cell.WarningViewModel.Warnings = new[] { warning };
                    localWarnings.Add(warning);
                }
            }

            var allWarnings = localWarnings.Concat(AddAdditionalWarnings(m_Context.Achievements));
            HeaderViewModel.Warnings = allWarnings.ToArray();
        }

        protected virtual string[] AddAdditionalWarnings(IReadOnlyList<IAchievement> achievements)
        {
            return Array.Empty<string>();
        }
    }
}
