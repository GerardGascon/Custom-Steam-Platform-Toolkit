using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeControlsWindow : EditorWindow
    {
        private PlayModeControlsSettingsField m_SettingsField;
        private PlayModeControls m_PlayModeControls = null;
        private PlayModeTestAccountDataView m_TestAccountDataView = null;
        private PlayModeControlsPlatformSelectField m_PlatformSelectField;

        private TabView m_TabView;
        private ScrollView m_TestAccountDataContent;

        [MenuItem("Window/Platform Toolkit/Play Mode Controls", priority = Editor.MenuPriority.PlayModeControls)]
        static void ShowPlayModeControlsWindow()
        {
            var window = GetWindow<PlayModeControlsWindow>();
            window.titleContent = new GUIContent("Play Mode Controls");
        }

        private void CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/Editor/PlayMode/UI/Controls/PlayModeControlsWindowRoot.uxml");
            rootVisualElement.Add(visualTreeAsset.CloneTree());

            m_SettingsField = rootVisualElement.Q<PlayModeControlsSettingsField>("play-mode-controls-settings-field");

            // Platform behaviour dropdown
            m_PlatformSelectField = new PlayModeControlsPlatformSelectField();
            rootVisualElement.Q<VisualElement>("header-section").Add(m_PlatformSelectField);

            m_TabView = rootVisualElement.Q<TabView>();

            var playModeControlsTab = m_TabView.Q<Tab>("play-mode-controls__tab");
            m_TestAccountDataContent = m_TabView.Q<Tab>("test-account-data__tab").Q<ScrollView>();

            if (m_PlayModeControls == null)
            {
                m_PlayModeControls = new PlayModeControls(playModeControlsTab.Q<VisualElement>("play-mode-controls-view"));
            }

            if (m_TestAccountDataView == null)
            {
                m_TestAccountDataView = new PlayModeTestAccountDataView(showPlatformField: false);
            }
            RefreshUI();
            RefreshEnableStates(EditorApplication.isPlaying);
        }

        void RefreshUI()
        {
            if (PlayModeControlsEditorSettings.instance.CurrentSettings is { } settings &&
                settings.ViewModel != null && settings.ViewModel.IsValid)
            {
                rootVisualElement.dataSource = settings.ViewModel;
                m_PlayModeControls.BindElements();
                m_SettingsField.SetVisibility(visible: false);
                m_TestAccountDataContent.Clear();
                m_TestAccountDataView.Unbind();
                m_PlatformSelectField.RefreshViewModel(settings.ViewModel);
                m_PlatformSelectField.SetVisibility(visible: true);
                m_TestAccountDataView.SetPlayModeControlsViewModel(settings.ViewModel);
                m_TestAccountDataContent.Add(m_TestAccountDataView.BuildView());
                m_TestAccountDataView.SetVisibility(visible: true);
                m_TabView.style.display = DisplayStyle.Flex;
            }
            else
            {
                rootVisualElement.dataSource = null;
                m_PlayModeControls.UnbindElements();
                m_SettingsField.SetVisibility(visible: true);
                m_PlatformSelectField.SetVisibility(visible: false);
                m_TestAccountDataView.SetVisibility(visible: false);
                m_TestAccountDataView.Unbind();
                m_TabView.style.display = DisplayStyle.None;
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnPlayModeStateChanged;
            PlayModeControlsEditorSettings.instance.OnSettingsAssetChange += RefreshUI;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnPlayModeStateChanged;
            PlayModeControlsEditorSettings.instance.OnSettingsAssetChange -= RefreshUI;
            m_PlatformSelectField?.Dispose();
        }

        private void RefreshEnableStates(bool isPlaying)
        {
            m_SettingsField.SetEnabled(!isPlaying);
            m_PlatformSelectField.SetEnabled(!isPlaying);
            m_TestAccountDataView.SetEnabled(!isPlaying);
        }

        private void EditorApplicationOnPlayModeStateChanged(PlayModeStateChange playModeChange)
        {
            if (m_SettingsField == null)
                return;

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
