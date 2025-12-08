using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class AttributeSettingsField : VisualElement
    {
        private AttributeSettingsViewModel m_SettingsViewModel;

        public AttributeSettingsField(AttributeSettings settings)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/EditorResources/UI/AttributeSettingsField.uxml");
            uxml.CloneTree(this);

            m_SettingsViewModel = new AttributeSettingsViewModel(settings);

            var customAttributes = this.Q<ListView>("custom-attributes");
            customAttributes.onAdd += list => settings.Add();
            customAttributes.onRemove += list =>
            {
                var index = list.selectedIndex < 0 ? list.childCount : list.selectedIndex;
                settings.RemoveAt(index);

                // This is not automatically reset after refreshing, so we have to do it manually to prevent errors
                list.selectedIndex = -1;
                // You get a warning messages when you do not refresh
                list.RefreshItems();
            };

            dataSource = m_SettingsViewModel;
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent e)
        {
            m_SettingsViewModel.Dispose();
        }
    }
}
