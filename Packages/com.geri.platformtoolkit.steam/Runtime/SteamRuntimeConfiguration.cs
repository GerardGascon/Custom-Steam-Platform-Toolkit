using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamRuntimeConfiguration : BaseRuntimeConfiguration {
		public AttributeStore attributes;

		public override IPlatformToolkit InstantiatePlatformToolkit() => new SteamPlatformToolkit(attributes);
	}
}