using System.Threading.Tasks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamPlatformToolkit : IPlatformToolkit {
		public ICapabilities Capabilities { get; }

		public SteamPlatformToolkit(AttributeStore attributes) {
			//TODO: Attributes should be stored somewhere?
		}

		public Task Initialize() {
			//TODO: Initialize steamworks here
			return Task.CompletedTask;
		}
	}
}