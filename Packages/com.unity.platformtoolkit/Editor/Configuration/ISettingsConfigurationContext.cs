namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Passed to <see cref="IPlatformToolkitSettingsProvider.CreateSettingsConfiguration"/> to allow the settings configuration access to its serialized data.</summary>
    internal interface ISettingsConfigurationContext
    {
        /// <summary>Sets the serialized settings data for the configuration. The data will be stored.</summary>
        /// <param name="serializedSettings"></param>
        void SetSerializedSettings(string serializedSettings);

        /// <summary>Gets the data that was previously set via <see cref="SetSerializedSettings"/>.</summary>
        /// <param name="serializedSettings">Serialized settings.</param>
        /// <returns>False if settings were never set.</returns>
        bool TryGetSerializedSettings(out string serializedSettings);
    }
}
