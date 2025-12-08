namespace Unity.PlatformToolkit.Editor
{
    internal class SettingsConfigurationContext : ISettingsConfigurationContext
    {
        private readonly string m_ImplementationKey;
        private readonly StoredSettings m_StoredSettings;

        public SettingsConfigurationContext(string implementationKey, StoredSettings storedSettings)
        {
            m_ImplementationKey = implementationKey;
            m_StoredSettings = storedSettings;
        }

        public void SetSerializedSettings(string serializedSettings)
        {
            m_StoredSettings.SetConfigurationData(m_ImplementationKey, serializedSettings);
        }

        public bool TryGetSerializedSettings(out string serializedSettings)
        {
            return m_StoredSettings.TryGetImplementationData(m_ImplementationKey, out serializedSettings);
        }
    }
}
