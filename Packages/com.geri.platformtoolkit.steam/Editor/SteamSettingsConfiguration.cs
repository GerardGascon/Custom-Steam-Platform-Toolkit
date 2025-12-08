using Unity.PlatformToolkit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Geri.PlatformToolkit.Steam.Editor {
	internal class SteamSettingsConfiguration : ISettingsConfiguration {
		private readonly ISettingsConfigurationContext _context;
		private readonly SteamPlatformToolkitSettings _settings;
		public object Settings => _settings;

		public SteamSettingsConfiguration(ISettingsConfigurationContext context) {
			_context = context;
			_settings = !context.TryGetSerializedSettings(out string settings)
				? new SteamPlatformToolkitSettings()
				: JsonUtility.FromJson<SteamPlatformToolkitSettings>(settings);

			_settings.SteamAttributeSettings.SettingsChanged += SaveSettings;
		}

		private void SaveSettings() {
			_context.SetSerializedSettings(JsonUtility.ToJson(_settings));
		}

		public VisualElement CreateSettingsUI() {
			return new AttributeSettingsField(_settings.SteamAttributeSettings);
		}
	}
}