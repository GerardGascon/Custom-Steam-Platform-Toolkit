using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class AttributeSettingsViewModel : IDisposable
    {
        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            var showWarningConverter = new ConverterGroup("Show Warning Converter");
            var showStyleEnum = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            var hideStyleEnum = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            showWarningConverter.AddConverter((ref bool val) => val ? showStyleEnum : hideStyleEnum);
            ConverterGroups.RegisterConverterGroup(showWarningConverter);
        }

        private AttributeSettings m_Settings;

        [CreateProperty]
        public List<AttributeViewModel> AttributeViewModels;
        [CreateProperty]
        public bool ShowWarning;

        public AttributeSettingsViewModel(AttributeSettings settings)
        {
            m_Settings = settings;

            AttributeViewModels = settings.Attributes.Select(a => new AttributeViewModel(settings, a)).ToList();

            m_Settings.AttributeAdded += OnAttributeAdded;
            m_Settings.AttributeRemoved += OnAttributeRemoved;
            m_Settings.SettingsChanged += OnAttributeModified;
        }

        private void OnAttributeAdded(IAttribute attribute)
        {
            AttributeViewModels.Add(new AttributeViewModel(m_Settings, attribute));
        }

        private void OnAttributeRemoved(IAttribute attribute)
        {
            var viewModel = AttributeViewModels.First(amv => amv.Attribute == attribute);
            AttributeViewModels.Remove(viewModel);
        }

        private void OnAttributeModified()
        {
            var duplicateNames = m_Settings.Attributes.GroupBy(a => a.Name).Where(g => g.Count() > 1).ToList();
            if (!duplicateNames.Any() || (duplicateNames.Count() == 1 && string.IsNullOrEmpty(duplicateNames.First().Key)))
                ShowWarning = false;
            else
                ShowWarning = true;
        }

        // TODO use WeakEvent (once it lands) and get rid of this Dispose
        public void Dispose()
        {
            m_Settings.AttributeAdded -= OnAttributeAdded;
            m_Settings.AttributeRemoved -= OnAttributeRemoved;
            m_Settings.SettingsChanged -= OnAttributeModified;
        }
    }
}
