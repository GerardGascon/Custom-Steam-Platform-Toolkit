using System.Collections.Generic;
using Unity.PlatformToolkit.Editor;
using UnityEditor;

namespace Unity.PlatformToolkit.LocalSaving.Editor
{
    internal class LocalSavingSupportDeclaration : IPlatformToolkitSupportDeclaration
    {
        private static readonly BuildTarget[] k_SupportedBuildTargets = new[]
        {
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneLinux64
        };

        public string DisplayName => "Local Saving";
        public string Key => "Unity.LocalSaving";
        public IReadOnlyCollection<BuildTarget> SupportedPlatforms => k_SupportedBuildTargets;

        public IPlatformToolkitBuilder CreateBuilder(IAchievementConfigurationContext achievementContext, ISettingsConfigurationContext settingsContext)
        {
            return new LocalSavingBuilder();
        }
    }
}
