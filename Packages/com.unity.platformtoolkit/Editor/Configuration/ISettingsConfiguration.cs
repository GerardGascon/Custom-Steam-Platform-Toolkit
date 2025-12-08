using System;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Settings instance and additional functionality to manage display and storage of settings.</summary>
    internal interface ISettingsConfiguration
    {
        /// <summary>Settings object which is returned by <see cref="PlatformToolkitEditor.TryGetSettings{TSettings}"/>.</summary>
        /// <remarks>The actual type of this object must be castable to <see cref="IPlatformToolkitSettingsProvider.SettingsType"/> of the provider that created this configuration.</remarks>
        object Settings { get; }

        /// <summary>Create a view for the settings.</summary>
        /// <returns>View object.</returns>
        VisualElement CreateSettingsUI();
    }
}
