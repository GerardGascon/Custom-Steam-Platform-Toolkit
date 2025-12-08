using System.IO;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Unity.PlatformToolkit.Editor
{
    internal class AchievementEditorWindow : EditorWindow
    {
        private AchievementEditor m_AchievementEditor;
        private MultiColumnListView m_MultiColumnListView;
        private const int k_ColumnWidth = 250;
        [MenuItem("Window/Platform Toolkit/Achievement Editor", priority = Editor.MenuPriority.AchievementEditor)]
        private static void CreateWindow()
        {
            GetWindow<AchievementEditorWindow>().titleContent = new GUIContent("Achievement Editor");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
        }

        private void CreateGUI()
        {
            m_AchievementEditor = new AchievementEditor(PlatformToolkitSettings.instance.StoredAchievements, SupportDeclarationManager.SupportDeclarations);
            m_AchievementEditor.Achievements.CollectionChanged += AchievementsOnCollectionChanged;
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/Editor/Achievement/UI/AchievementEditor.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);
            m_MultiColumnListView = rootVisualElement.Q<MultiColumnListView>("achievement-list");
            var listAdapter = new ListAdapter<StoredAchievement>(m_AchievementEditor.Achievements, () => new StoredAchievement());
            m_MultiColumnListView.itemsSource = listAdapter;
            m_MultiColumnListView.columns.Add(CreateCommonAchievementColumn());

            foreach (var configurationAndContext in m_AchievementEditor.ConfigurationsAndContext)
            {
                var column = new Column
                {
                    stretchable = true, minWidth = k_ColumnWidth, name = configurationAndContext.configuration.PlatformName,
                };
                column.makeHeader += configurationAndContext.configuration.EditorProvider.MakeHeader;
                column.makeCell += configurationAndContext.configuration.EditorProvider.MakeCell;
                column.bindCell += (cell, i) =>
                {
                    configurationAndContext.configuration.EditorProvider.BindCell(cell, configurationAndContext.context.Achievements[i]);
                };
                column.unbindCell += (cell, _) =>
                {
                    configurationAndContext.configuration.EditorProvider.UnbindCell(cell);
                };

                m_MultiColumnListView.columns.Add(column);
            }

            var btnImport = rootVisualElement.Q<Button>("btn-import");
            btnImport.clickable.clicked += Import;

            var btnExport = rootVisualElement.Q<Button>("btn-export");
            btnExport.clickable.clicked += Export;
            btnExport.dataSource = m_AchievementEditor;

            RefreshEnableStates(EditorApplication.isPlaying);
        }

        private void Import()
        {
            var filePath = EditorUtility.OpenFilePanel("Select file to import", Application.dataPath, "*csv");
            if (!File.Exists(filePath)) return;
            var data = File.ReadAllText(filePath);
            m_AchievementEditor.ImportCsv(data);
        }

        private void AchievementsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            m_MultiColumnListView.RefreshItems();
        }

        private void Export()
        {
            var path = EditorUtility.SaveFilePanel("Select folder/file to export file to", Application.dataPath, "PT-Achievements","csv");
            if (string.IsNullOrEmpty(path))
                return;
            var data = m_AchievementEditor.Export();
            File.WriteAllText(path, data);
        }

        private Column CreateCommonAchievementColumn()
        {
            var commonColumn = new Column
            {
                name = "Platform Toolkit",
                stretchable = true,
                makeHeader = () => m_AchievementEditor.CommonConfiguration.MakeHeader(),
                makeCell = () => m_AchievementEditor.CommonConfiguration.MakeCell(),
                bindCell = (cell, i) =>
                {
                    m_AchievementEditor.CommonConfiguration.BindCell(cell, m_AchievementEditor.Achievements[i]);
                },
                unbindCell = (cell, _) =>
                {
                    m_AchievementEditor.CommonConfiguration.UnbindCell(cell);
                },
                minWidth = k_ColumnWidth
            };
            return commonColumn;
        }

        private void RefreshEnableStates(bool isPlaying)
        {
            rootVisualElement.SetEnabled(!isPlaying);
        }

        private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange playModeChange)
        {
            switch (playModeChange)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    RefreshEnableStates(true);
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    RefreshEnableStates(false);
                    break;
            }
        }
    }
}
