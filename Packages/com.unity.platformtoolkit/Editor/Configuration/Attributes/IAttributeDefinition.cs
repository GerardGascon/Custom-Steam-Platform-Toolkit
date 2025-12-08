using System;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Attribute definition exposed by a Platform Toolkit implementation.</summary>
    public interface IAttributeDefinition
    {
        /// <summary>Unique identifier of the attribute definition.</summary>
        public string Id { get; }

        /// <summary>Type of the attribute.</summary>
        public Type Type { get; }

        /// <summary>Display name of the attribute.</summary>
        public string Name { get; }
    }
}
