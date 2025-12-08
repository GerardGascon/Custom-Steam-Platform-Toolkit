using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Editor UI (part of a custom Inspector) for play mode save games.
    /// </summary>
    [UxmlElement]
    internal partial class PlayModeControlsSaveDataField : VisualElement
    {
        private PlayModeControlsViewModel m_PlayModeControlsView;
        private ListView m_SaveList;
        private Button m_SaveImportButton;
        private VisualElement m_SaveCapabilityWarning;
        private Label m_SaveCapabilityWarningText;
        private EventCallback<ClickEvent> m_SaveImportCallback;

        private bool m_IsPerAccountSave;

        public void Init(PlayModeControlsViewModel playModeControlsView, bool isPerAccountSave)
        {
            Assert.IsNull(m_PlayModeControlsView, "m_PlayModeControlsView is not null. init should not be called more than once.");
            Assert.IsNotNull(playModeControlsView);

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/Editor/Playmode/UI/Inspector/PlayModeControlsSaveDataList.uxml");
            uxml.CloneTree(this);

            m_PlayModeControlsView = playModeControlsView;
            m_IsPerAccountSave = isPerAccountSave;

            m_SaveList = this.Q<ListView>("SaveListView");
            m_SaveImportButton = this.Q<Button>("NewSaveImportButton");
            m_SaveCapabilityWarning = this.Q<VisualElement>("SupportWarning");
            m_SaveCapabilityWarningText = this.Q<Label>("SupportWarningText");

            m_PlayModeControlsView.OnCapabilitiesInvalidated.AddWeakListener(OnCapabilitiesChanged);
        }

        public void Bind(PlayModeSaveData saveData, string foldoutName)
        {
            Assert.IsNotNull(m_PlayModeControlsView);

            m_SaveList.headerTitle = foldoutName;

            // itemSource property is bound in UXML
            m_SaveList.bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;
            m_SaveList.dataSource = saveData;
            m_SaveList.makeItem = () => new PlayModeControlsSaveDataInfoField(saveData);
            m_SaveList.bindItem = (item, index) =>
            {
                var source = m_SaveList.itemsSource as List<PlayModeSaveDataInfo>;
                ((PlayModeControlsSaveDataInfoField)item).Bind(source[index]);
            };

            SetImportCallback(saveData);

            RefreshVisibility();
        }

        private void SetImportCallback(PlayModeSaveData saveData)
        {
            if (m_SaveImportCallback != null)
            {
                m_SaveImportButton.UnregisterCallback<ClickEvent>(m_SaveImportCallback);
                m_SaveImportCallback = null;
            }

            var saveImportExportHelper = new PlayModeImportExportSave(saveData);
            m_SaveImportCallback = async _ =>
            {
                var selectedFolder = EditorUtility.OpenFilePanel("Select a Platform Toolkit zip file to add its content as a new save", Application.dataPath, "zip");
                if (string.IsNullOrEmpty(selectedFolder))
                    return;

                await saveImportExportHelper.AddNewSaveFromDisk(selectedFolder);
            };

            m_SaveImportButton.RegisterCallback<ClickEvent>(m_SaveImportCallback);
        }

        private void OnCapabilitiesChanged()
        {
            RefreshVisibility();
        }

        private void RefreshVisibility()
        {
            if (m_IsPerAccountSave)
            {
                if (!m_PlayModeControlsView.SupportsAccounts)
                {
                    m_SaveCapabilityWarning.style.display =  DisplayStyle.Flex;
                    m_SaveCapabilityWarningText.text = "This Behaviour does not support save data per-account.";
                }
                else
                {
                    m_SaveCapabilityWarning.style.display = DisplayStyle.None;
                }
            }
            else
            {
                if (!m_PlayModeControlsView.SupportsLocalSaving)
                {
                    m_SaveCapabilityWarning.style.display = DisplayStyle.Flex;
                    m_SaveCapabilityWarningText.text = "This Behaviour does not support non-account based save data.";
                }
                else
                {
                    m_SaveCapabilityWarning.style.display = DisplayStyle.None;
                }
            }
        }

        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            var infoImage = new ConverterGroup("Save Info Image To Texture2D Converter");
            infoImage.AddConverter((ref byte[] imageInfo) =>
            {
                if (imageInfo == null || imageInfo.Length == 0)
                {
                    return new StyleBackground();
                }

                Texture2D tex = new(2, 2);
                tex.LoadImage(imageInfo);

                return new StyleBackground(tex);
            });

            var defaultImageDisplay = new ConverterGroup("Default Image Display");
            defaultImageDisplay.AddConverter((ref byte[] imageInfo) =>
            {
                return new StyleEnum<DisplayStyle>(imageInfo == null || imageInfo.Length == 0 ? DisplayStyle.None : DisplayStyle.Flex);
            });

            var defaultImageHide = new ConverterGroup("Default Image Hide");
            defaultImageHide.AddConverter((ref byte[] imageInfo) =>
            {
                return new StyleEnum<DisplayStyle>(imageInfo == null || imageInfo.Length == 0 ? DisplayStyle.Flex : DisplayStyle.None);
            });

            var defaultTextDisplay = new ConverterGroup("Default Text Hide");
            defaultTextDisplay.AddConverter((ref string text) =>
            {
                return new StyleEnum<DisplayStyle>(string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex);
            });

            var defaultTextHide = new ConverterGroup("Default Text Display");
            defaultTextHide.AddConverter((ref string text) =>
            {
                return new StyleEnum<DisplayStyle>(string.IsNullOrEmpty(text) ? DisplayStyle.Flex : DisplayStyle.None);
            });

            ConverterGroups.RegisterConverterGroup(infoImage);
            ConverterGroups.RegisterConverterGroup(defaultImageDisplay);
            ConverterGroups.RegisterConverterGroup(defaultImageHide);
            ConverterGroups.RegisterConverterGroup(defaultTextDisplay);
            ConverterGroups.RegisterConverterGroup(defaultTextHide);
        }
    }
}
