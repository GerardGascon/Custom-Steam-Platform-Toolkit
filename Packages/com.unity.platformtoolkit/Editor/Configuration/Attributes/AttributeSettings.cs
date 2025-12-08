using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal abstract class AttributeSettings : IAttributeSettings, ISerializationCallbackReceiver
    {
        public event Action SettingsChanged;
        public event Action<IAttribute> AttributeAdded;
        public event Action<IAttribute> AttributeRemoved;

        private class AttributeDefinition : IAttributeDefinition
        {
            public AttributeDefinition(string id, Type type, string name)
            {
                Id = id;
                Type = type;
                Name = name;
            }

            public string Id { get; }
            public Type Type { get; }
            public string Name { get; }
        }

        public IReadOnlyList<IAttributeDefinition> AttributeDefinitions => m_AttributeDefinitions;
        private List<string> m_AttributeIds;

        public IReadOnlyList<IAttribute> Attributes => attributes;

        public IAttribute Add()
        {
            var newAttribute = new Attribute(m_AttributeIds);
            newAttribute.propertyChanged += AttributeChanged;
            attributes.Add(newAttribute);
            SettingsChanged?.Invoke();
            AttributeAdded?.Invoke(newAttribute);
            return newAttribute;
        }

        public void RemoveAt(int index)
        {
            var attribute = attributes[index];
            attribute.propertyChanged -= AttributeChanged;
            attributes.RemoveAt(index);
            SettingsChanged?.Invoke();
            AttributeRemoved?.Invoke(attribute);
        }

        public AttributeStore BuildAttributes()
        {
            var attributes = new List<string>();
            var names = new List<string>();

            var usedNames = new HashSet<string>();
            foreach (var attribute in this.attributes)
            {
                if (!usedNames.Add(attribute.Name))
                    throw new InvalidDataException($"Multiple attributes with the same name {attribute.Name}");

                attributes.Add(attribute.Id);
                names.Add(attribute.Name);
            }
            return new AttributeStore(attributes, names);
        }

        protected AttributeSettings()
        {
            InitializeAttributes(out var attributeDefinitions);

            var usedAttributeIds = new HashSet<string>();
            foreach (var attributeDefinition in attributeDefinitions)
            {
                if (!usedAttributeIds.Add(attributeDefinition.AttributeId))
                    throw new ArgumentException("Duplicate attribute definition", nameof(attributeDefinition));

                m_AttributeDefinitions.Add(new AttributeDefinition(attributeDefinition.AttributeId, attributeDefinition.AttributeType, attributeDefinition.AttributeName));
            }

            m_AttributeIds = m_AttributeDefinitions.Select(a => a.Id).ToList();
        }

        protected abstract void InitializeAttributes(out IReadOnlyList<(string AttributeId, Type AttributeType, string AttributeName)> attributeDefinitions);

        protected void OnSettingsChanged()
        {
            SettingsChanged?.Invoke();
        }

        private List<AttributeDefinition> m_AttributeDefinitions = new List<AttributeDefinition>();

        [SerializeField]
        private List<Attribute> attributes = new List<Attribute>();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            foreach (var namedAttribute in attributes)
            {
                namedAttribute.propertyChanged += AttributeChanged;
                namedAttribute.AttributeIds = m_AttributeIds;
            }
        }

        private void AttributeChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            SettingsChanged?.Invoke();
        }

        [Serializable]
        private class Attribute : IAttribute, INotifyBindablePropertyChanged
        {
            public IReadOnlyList<string> AttributeIds { get; set; }
            public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

            public Attribute(IReadOnlyList<string> attributeIds)
            {
                if (attributeIds == null)
                    throw new ArgumentNullException(nameof(attributeIds));
                if (attributeIds.Count == 0)
                    throw new ArgumentException("Attribute Id list must not be empty", nameof(attributeIds));
                if (attributeIds.Any(string.IsNullOrEmpty))
                    throw new ArgumentException("Attribute Ids must not be null or empty", nameof(attributeIds));

                AttributeIds = attributeIds;
                attributeId = AttributeIds[0];
                name = string.Empty;
            }

            [SerializeField, DontCreateProperty]
            private string attributeId;

            public string Id
            {
                get => attributeId;
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value), $"{nameof(Id)} cannot be null.");
                    }
                    if (AttributeIds.Contains(value))
                    {
                        if (attributeId != value)
                        {
                            attributeId = value;
                            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(Id)));
                        }
                    }
                    else
                        throw new ArgumentException("Attribute Id not recognized.", nameof(value));
                }
            }

            [SerializeField, DontCreateProperty]
            private string name;

            public string Name
            {
                get => name;
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException(nameof(value), $"{nameof(Name)} cannot be null.");
                    }
                    if (value != name)
                    {
                        name = value;
                        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(Name)));
                    }
                }
            }
        }
    }
}
