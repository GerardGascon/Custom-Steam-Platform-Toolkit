using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.PlatformToolkit.PlayMode
{
    [UxmlElement]
    internal partial class PlayModeControlsSettingsField : VisualElement
    {
        private ObjectField m_SettingsField;
        private Label m_PMCLabel;
        private Button m_CreateSettingsButton;
        private HelpBox m_HelpBox = new HelpBox("Create a Play Mode Controls asset to see test account data.", HelpBoxMessageType.Info);

        public PlayModeControlsSettingsField()
        {
            m_SettingsField = new ObjectField
            {
                objectType = typeof(PlayModeControlsSettings)
            };
            Add(m_SettingsField);
            m_SettingsField.SetValueWithoutNotify(PlayModeControlsEditorSettings.instance.CurrentSettings);
            m_SettingsField.RegisterValueChangedCallback(OnPlayModeControlsSettingsSet);

            m_CreateSettingsButton = new Button
            {
                text = "Create Play Mode Controls Settings"
            };
            m_CreateSettingsButton.clicked += CreateSettingsAsset;
            Add(m_CreateSettingsButton);
            Add(m_HelpBox);
        }

        /// <summary>
        /// Should be set to visible when no asset is set.
        /// </summary>
        /// <param name="visible"></param>
        public void SetVisibility(bool visible)
        {
            if (visible)
            {
                m_CreateSettingsButton.style.display = DisplayStyle.Flex;
                m_HelpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_CreateSettingsButton.style.display = DisplayStyle.None;
                m_HelpBox.style.display = DisplayStyle.None;
            }
        }

        private void CreateSettingsAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Play Mode Controls Settings", "PlayModeControlsSettings", "asset", "");
            if (string.IsNullOrEmpty(path))
                return;

            var asset = ScriptableObject.CreateInstance<PlayModeControlsSettings>();
            AssetDatabase.CreateAsset(asset, path);
            m_SettingsField.value = asset;
        }

        private void OnPlayModeControlsSettingsSet(ChangeEvent<Object> evt)
        {
            PlayModeControlsEditorSettings.instance.CurrentSettings = (PlayModeControlsSettings)evt.newValue;
        }
    }
}
