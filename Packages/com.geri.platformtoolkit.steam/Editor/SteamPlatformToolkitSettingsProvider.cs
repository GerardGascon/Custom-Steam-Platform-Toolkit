using System;
using Unity.PlatformToolkit.Editor;

namespace Geri.PlatformToolkit.Steam.Editor {
	internal class SteamPlatformToolkitSettingsProvider : IPlatformToolkitSettingsProvider {
		public Type SettingsType => typeof(ISteamPlatformToolkitSettings);
		public ISettingsConfiguration CreateSettingsConfiguration(ISettingsConfigurationContext context) {
			return new SteamSettingsConfiguration(context);
		}
	}
}