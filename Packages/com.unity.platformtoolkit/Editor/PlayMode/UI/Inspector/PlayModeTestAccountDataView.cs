using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeTestAccountDataView
    {
        private VisualElement m_InspectorRoot = new VisualElement();
        private PlayModeControlsPlatformSelectField m_PlatformSelectField;
        private PlayModeControlsViewModel m_PlayModeControlsViewModel;
        private bool m_ShowPlatformField;

        private const string kStyleSheet = "Packages/com.unity.platformtoolkit/Editor/PlayMode/UI/Styles.uss";

        public PlayModeTestAccountDataView(bool showPlatformField)
        {
            m_ShowPlatformField = showPlatformField;
        }

        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                m_InspectorRoot.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_InspectorRoot.style.display = DisplayStyle.None;
            }
        }

        public void SetEnabled(bool enabled)
        {
            m_InspectorRoot.SetEnabled(enabled);
        }

        public void SetPlayModeControlsViewModel(PlayModeControlsViewModel playModeControlsViewModel)
        {
            m_PlayModeControlsViewModel = playModeControlsViewModel;
            m_InspectorRoot.dataSource = m_PlayModeControlsViewModel;
        }

        public VisualElement BuildView()
        {
            Unbind();

            if (m_PlayModeControlsViewModel == null || !m_PlayModeControlsViewModel.IsValid)
                return null;

            var styleSheet = EditorGUIUtility.Load(kStyleSheet) as StyleSheet;
            m_InspectorRoot.styleSheets.Add(styleSheet);
            m_InspectorRoot.style.minWidth = 431;

            if (m_ShowPlatformField)
            {
                m_PlatformSelectField = new PlayModeControlsPlatformSelectField();
                m_PlatformSelectField.RefreshViewModel(m_PlayModeControlsViewModel);
                m_InspectorRoot.Add(m_PlatformSelectField);
            }

            var accountSection = new VisualElement();
            accountSection.Add(new PlayModeControlsAccountDataList(m_PlayModeControlsViewModel));

            m_InspectorRoot.Add(accountSection);
            m_InspectorRoot.SetEnabled(!EditorApplication.isPlaying);

            var saveDataField = new PlayModeControlsSaveDataField();
            saveDataField.Init(m_PlayModeControlsViewModel, isPerAccountSave: false);
            saveDataField.Bind(m_PlayModeControlsViewModel.LocalSaveData, "Local saves");
            m_InspectorRoot.Add(saveDataField);
            return m_InspectorRoot;
        }

        public void Unbind()
        {
            m_InspectorRoot?.Clear();
        }
    }
}
