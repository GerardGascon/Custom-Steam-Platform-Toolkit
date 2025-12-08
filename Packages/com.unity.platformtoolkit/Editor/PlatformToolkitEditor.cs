using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>
    /// Provides access to Platform Toolkit settings for the editor.
    /// Also provides a public API for managing Platform Toolkit configurations at the project level.
    /// This allows programmatically setting and querying which Platform Toolkit implementation
    /// is active for different Unity build targets. Useful for build pipelines and custom tooling.
    /// </summary>
    public static class PlatformToolkitEditor
    {
        /// <summary>Get an editable instance of project settings for a Platform Toolkit implementation.</summary>
        /// <param name="settings">Settings object.</param>
        /// <typeparam name="TSettings">Type of settings to be retrieved.</typeparam>
        /// <remarks>
        /// <para>Platform Toolkit implementation may expose a settings type, in which case an object of that type will be retrievable via this method.</para>
        /// <para>Settings type for each implementation is documented in the corresponding implementation documentation.</para>
        /// </remarks>
        /// <returns>
        /// <para>True, if an implementation is found that is using <see cref="TSettings"/> as its settings type.</para>
        /// <para>False, if no implementation is found that would be using <see cref="TSettings"/> as its settings type.</para>
        /// </returns>
        public static bool TryGetSettings<TSettings>(out TSettings settings)
        {
            if (!SupportDeclarationManager.TryGetDeclarationKey(typeof(TSettings), out var declarationKey))
            {
                settings = default;
                return false;
            }

            var settingsConfiguration = PlatformToolkitSettings.instance.GetSettingsConfiguration(declarationKey);
            if (settingsConfiguration == null)
            {
                settings = default;
                return false;
            }

            settings = (TSettings)settingsConfiguration.Settings;
            return true;
        }

        /// <summary>
        /// Represents information about an available Platform Toolkit implementation.
        /// </summary>
        public class PlatformToolkitImplementationInfo
        {
            internal PlatformToolkitImplementationInfo(IPlatformToolkitSupportDeclaration supportDeclaration)
            {
                m_SupportDeclaration = supportDeclaration;
            }

            private readonly IPlatformToolkitSupportDeclaration m_SupportDeclaration;
            /// <summary>
            /// The unique identifier for this Platform Toolkit implementation.
            /// This key is used when setting configurations.
            /// </summary>
            public string Key => m_SupportDeclaration.Key;

            /// <summary>
            /// The display name for this Platform Toolkit implementation, as shown in the Unity Editor UI.
            /// </summary>
            public string DisplayName => m_SupportDeclaration.DisplayName;

            /// <summary>
            /// A read-only collection of Unity <see cref="BuildTarget"/> that this Platform Toolkit implementation supports.
            /// </summary>
            public IReadOnlyCollection<BuildTarget> SupportedPlatforms => m_SupportDeclaration.SupportedPlatforms;
        }

        /// <summary>Gets a list of all available Platform Toolkit implementations (support declarations) detected in the project.</summary>
        /// <returns>A read-only list of <see cref="PlatformToolkitImplementationInfo"/> for all available implementations.</returns>
        public static IReadOnlyList<PlatformToolkitImplementationInfo> GetAvailableImplementations()
        {
            List<PlatformToolkitImplementationInfo> availableConfigurations = new List<PlatformToolkitImplementationInfo>();
            foreach (IPlatformToolkitSupportDeclaration supportDeclaration in SupportDeclarationManager.SupportDeclarations)
            {
                availableConfigurations.Add(new PlatformToolkitImplementationInfo(supportDeclaration));
            }

            return availableConfigurations;
        }

        /// <summary>
        /// Configures a specific Platform Toolkit implementation to be used for a given Unity <see cref="BuildTarget"/>.
        /// This will overwrite any existing configuration for the specified build target.
        /// </summary>
        /// <param name="buildTarget">The Unity <see cref="BuildTarget"/> to configure (e.g., BuildTarget.Android, BuildTarget.iOS).</param>
        /// <param name="key">The unique key of the Platform Toolkit implementation to use for that build target.
        /// Use <see cref="GetAvailableImplementations"/> to find valid implementation keys.</param>
        /// <returns>
        /// <para>True if the configuration was successfully set, updated, or is already set to that implementation.</para>
        /// <para>False if the declaration key is invalid, or if the specified implementation does not support the given build target.</para>
        /// </returns>
        public static bool SetImplementation(BuildTarget buildTarget, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"[PlatformToolkitEditor] Invalid declaration key provided for BuildTarget '{buildTarget}'. Key cannot be null or empty.");
                return false;
            }

            if (!SupportDeclarationManager.TryGetSupportDeclaration(key, out var supportDeclaration))
            {
                Debug.Log($"[PlatformToolkitEditor] No Platform Toolkit implementation found for key '{key}'.");
                return false;
            }

            if (!supportDeclaration.SupportedPlatforms.Contains(buildTarget))
            {
                Debug.LogWarning($"[PlatformToolkitEditor] The Platform Toolkit implementation '{supportDeclaration.DisplayName}' (key: '{key}') does not support BuildTarget '{buildTarget}'. Configuration not set.");
                return false;
            }

            if (PlatformToolkitSettings.instance.SupportDeclarationTargetsManager.GetTargetedPlatforms(key).Contains(buildTarget))
            {
                Debug.Log($"[PlatformToolkitEditor] The requested Platform Toolkit implementation '{supportDeclaration.DisplayName}' (key: '{key}') is already configured for BuildTarget '{buildTarget}'.");
                return true;
            }

            bool success = PlatformToolkitSettings.instance.SupportDeclarationTargetsManager.TryAddBuildTarget(key, buildTarget);
            if (!success)
            {
                Debug.LogWarning($"[PlatformToolkitEditor] Failed to set configuration for BuildTarget '{buildTarget}' to '{key}'.");
            }
            return success;
        }

        /// <summary>
        /// Tries to get the <see cref="PlatformToolkitImplementationInfo"/> currently configured for a specific Unity <see cref="BuildTarget"/>.
        /// </summary>
        /// <param name="buildTarget">The Unity <see cref="BuildTarget"/> to query.</param>
        /// <param name="info">When this method returns, contains the info of the configured
        /// Platform Toolkit implementation, or null if no implementation info is found for the given build target.</param>
        /// <returns>True if an implementations info was found for the specified build target; otherwise, false.</returns>
        public static bool TryGetImplementationInfo(BuildTarget buildTarget, out PlatformToolkitImplementationInfo info)
        {
            if (PlatformToolkitSettings.instance.SupportDeclarationTargetsManager.TryGetDeclarationForBuildTarget(buildTarget, out var declarationKey))
            {
                foreach (var declaration in SupportDeclarationManager.SupportDeclarations)
                {
                    if (declaration.Key == declarationKey)
                    {
                        info = new(declaration);
                        return true;
                    }
                }

                Debug.LogWarning($"[PlatformToolkitEditor] Failed to retrieve implementation info for BuildTarget '{buildTarget}', unable to find a matching key for '{declarationKey}'.");
            }
            else
            {
                Debug.LogWarning($"[PlatformToolkitEditor] No Platform Toolkit implementation info found for BuildTarget '{buildTarget}'.");
            }

            info = null;
            return false;
        }
    }
}
