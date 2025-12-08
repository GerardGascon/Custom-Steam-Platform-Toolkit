using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeControlsPlatformSelectField : VisualElement, IDisposable
    {
        private DropdownField m_Dropdown;
        private PlayModeControlsViewModel m_PlayModeControlsView;
        private const string k_StyleSheet = "Packages/com.unity.platformtoolkit/Editor/PlayMode/UI/Styles.uss";

        public PlayModeControlsPlatformSelectField()
        {
            var styleSheet = EditorGUIUtility.Load(k_StyleSheet) as StyleSheet;
            m_Dropdown = new DropdownField("Behavior");
            m_Dropdown.AddToClassList("platform-select-field");
            m_Dropdown.RegisterValueChangedCallback(OnValueSelected);
            var label = m_Dropdown.Q<Label>();
            label.AddToClassList("title");
            m_Dropdown.styleSheets.Add(styleSheet);
            Add(m_Dropdown);
        }

        public void RefreshViewModel(PlayModeControlsViewModel playModeControlsView)
        {
            if (m_PlayModeControlsView != null)
            {
                m_PlayModeControlsView.OnCapabilitiesInvalidated.RemoveListener(RefreshSelection);
            }

            m_PlayModeControlsView = playModeControlsView;
            m_PlayModeControlsView.OnCapabilitiesInvalidated.AddWeakListener(RefreshSelection);
            RefreshSelection();
        }

        public void SetVisibility(bool visible)
        {
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnValueSelected(ChangeEvent<string> evt)
        {
            m_PlayModeControlsView.Capability = m_PlayModeControlsView.CapabilityOptions[evt.newValue];
        }

        private void RefreshSelection()
        {
            var currentCapability = m_PlayModeControlsView.Capability;

            if(m_Dropdown.value != null && m_PlayModeControlsView.CapabilityOptions.TryGetValue(m_Dropdown.value, out var capability) && capability == currentCapability)
                return;

            m_Dropdown.choices = m_PlayModeControlsView.CapabilityOptionNames;
            var key = m_PlayModeControlsView.CapabilityOptions.FirstOrDefault(kvp => kvp.Value == currentCapability).Key;
            if(key != null)
                m_Dropdown.value = key;
        }

        public void Dispose()
        {
            m_PlayModeControlsView?.OnCapabilitiesInvalidated.RemoveListener(RefreshSelection);
        }
    }
}
