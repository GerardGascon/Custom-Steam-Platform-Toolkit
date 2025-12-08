namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Attribute settings.</summary>
    public interface IAttribute
    {
        /// <summary>Attribute definition Id.</summary>
        /// <remarks>Available Ids can be found by looking up <see cref="IAttributeDefinition.Id"/> in <see cref="IAttributeSettings.AttributeDefinitions"/>.</remarks>
        public string Id { get; set; }
        /// <summary>Attribute name, used in <see cref="IAccount.GetAttribute{T}"/> method to retrieve attribute value.</summary>
        public string Name { get; set; }
    }
}
