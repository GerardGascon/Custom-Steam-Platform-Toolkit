using System.Collections.Generic;
using Unity.PlatformToolkit.Editor;
using UnityEditor;

namespace Geri.PlatformToolkit.Steam.Editor {
	internal class SteamSupportDeclaration : IPlatformToolkitSupportDeclaration {
		private static readonly BuildTarget[] SupportedBuildTargets = {
			BuildTarget.StandaloneWindows,
			BuildTarget.StandaloneWindows64,
			BuildTarget.StandaloneLinux64,
			BuildTarget.StandaloneOSX
		};

		public string DisplayName => "Custom Steam";
		public string Key => "Geri.Custom.Steam";

		public IReadOnlyCollection<BuildTarget> SupportedPlatforms => SupportedBuildTargets;

		private readonly SteamPlatformToolkitSettingsProvider _settingsProvider = new();
		public IPlatformToolkitSettingsProvider SettingsProvider => _settingsProvider;

		public IPlatformToolkitBuilder CreateBuilder(IAchievementConfigurationContext _,
			ISettingsConfigurationContext settingsContext) {
			ISettingsConfiguration settingsConfiguration =
				_settingsProvider.CreateSettingsConfiguration(settingsContext);
			SteamPlatformToolkitSettings settings = (SteamPlatformToolkitSettings)settingsConfiguration.Settings;
			return new SteamBuilder(settings, SupportedPlatforms);
		}
	}
}