using System;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Settings support.</summary>
    internal interface IPlatformToolkitSettingsProvider
    {
        /// <summary>Type that is used in <see cref="PlatformToolkitEditor.TryGetSettings{TSettings}"/>.</summary>
        Type SettingsType { get; }

        /// <summary>Create an instance of settings.</summary>
        /// <param name="context">Context object which allows retrieving and saving settings.</param>
        /// <returns>Settings configuration for an implementation.</returns>
        ISettingsConfiguration CreateSettingsConfiguration(ISettingsConfigurationContext context);
    }
}
