using System.Collections.Generic;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Attribute settings.</summary>
    public interface IAttributeSettings
    {
        /// <summary>List of attribute definitions supported by the Platform Toolkit implementation.</summary>
        public IReadOnlyList<IAttributeDefinition> AttributeDefinitions { get; }

        /// <summary>List of attributes.</summary>
        public IReadOnlyList<IAttribute> Attributes { get; }

        /// <summary>Add a new attribute to <see cref="Attributes"/>.</summary>
        /// <returns>New attribute.</returns>
        public IAttribute Add();

        /// <summary>Remove an attribute from <see cref="Attributes"/>.</summary>
        /// <param name="index">Index in <see cref="Attributes"/> list.</param>
        public void RemoveAt(int index);
    }
}
