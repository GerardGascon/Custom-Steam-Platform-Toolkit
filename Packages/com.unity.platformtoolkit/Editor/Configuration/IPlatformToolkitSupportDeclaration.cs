using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Entry point for Platform Toolkit implementations.</summary>
    internal interface IPlatformToolkitSupportDeclaration
    {
        /// <summary>Name that is displayed to identify the Platform Toolkit implementation.</summary>
        /// <remarks>This name is used in the editor, when setting up Platform Toolkit.</remarks>
        string DisplayName { get; }

        /// <summary>Key used to refer to an implementation internally.</summary>
        /// <remarks>Must be unique. If there are multiple support declarations with the same key, only one of them will be instantiated. It's recommended to us [CompanyName].[PlatformName] format.</remarks>
        string Key { get; }

        /// <summary>Build targets supported by the Platform Toolkit implementation.</summary>
        IReadOnlyCollection<BuildTarget> SupportedPlatforms { get; }

        /// <summary>Settings provider, null if implementation has no settings.</summary>
        IPlatformToolkitSettingsProvider SettingsProvider => null;

        /// <summary>Called before the build.</summary>
        /// <param name="achievementContext">Achievement context for the build.</param>
        /// <param name="settingsContext">Settings context for the build.</param>
        /// <returns>Builder for this Platform toolkit implementation.</returns>
        IPlatformToolkitBuilder CreateBuilder(IAchievementConfigurationContext achievementContext = null, ISettingsConfigurationContext settingsContext = null)
        {
            // TODO remove this default implementation, after all implementations have an implementation. Building is not optional.
            return null;
        }

        /// <summary>Indicates if achievements are supported by the Platform Toolkit implementation.</summary>
        /// <remarks>When the value is false, <see cref="CreateAchievementConfiguration"/> will should not be called.</remarks>
        bool AchievementsSupported => false;

        /// <summary>Create an IAchievementConfigurationContext for the Platform Toolkit implementation.</summary>
        /// <param name="context">Achievement context for the Platform Toolkit implementation.</param>
        /// <returns>New achievement configuration instance.</returns>
        IAchievementConfiguration CreateAchievementConfiguration(IAchievementConfigurationContext context)
        {
            throw new InvalidOperationException("Achievements are not supported by the Platform Toolkit implementation.");
        }
    }

    /// <summary>Contains methods to build implementation data, that implementation needs during runtime.</summary>
    internal interface IPlatformToolkitBuilder
    {
        /// <summary>Called before the build.</summary>
        /// <param name="buildReport">Build report for this build.</param>
        /// <returns>Asset that will be inserted into the build.</returns>
        BaseRuntimeConfiguration PrepareBuild(BuildReport buildReport);

        /// <summary>Called after the build.</summary>
        /// <param name="buildReport">Build report for this build.</param>
        void PostBuild(BuildReport buildReport);
    }
}
