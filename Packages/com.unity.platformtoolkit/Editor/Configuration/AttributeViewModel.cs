using System.Collections.Generic;
using System.Linq;
using Unity.Properties;

namespace Unity.PlatformToolkit.Editor
{
    internal class AttributeViewModel
    {
        [CreateProperty]
        public List<IAttributeDefinition> Choices => m_AttributeSettings.AttributeDefinitions.ToList();

        [CreateProperty]
        public string Name
        {
            get => Attribute.Name;
            set => Attribute.Name = value;
        }

        [CreateProperty]
        public IAttributeDefinition SelectedDefinition
        {
            get => m_AttributeDefinition;
            set
            {
                m_AttributeDefinition = value;
                Attribute.Id = value.Id;
            }
        }

        public IAttribute Attribute;
        private IAttributeDefinition m_AttributeDefinition;
        private AttributeSettings m_AttributeSettings;

        public AttributeViewModel(AttributeSettings settings, IAttribute attribute)
        {
            m_AttributeSettings = settings;
            m_AttributeDefinition = settings.AttributeDefinitions.First(ad => ad.Id == attribute.Id);

            Attribute = attribute;
        }
    }
}
