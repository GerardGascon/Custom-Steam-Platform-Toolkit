using System;
using System.Data;
using System.Threading.Tasks;
using Unity.PlatformToolkit;
using Steamworks;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamPlatformToolkit : IPlatformToolkit {
		public ICapabilities Capabilities { get; private set; }
		public IAccountSystem Accounts => _steamAccountSystem;

		private readonly SteamAccountSystem _steamAccountSystem;

		public SteamPlatformToolkit(AttributeStore attributes) {
			_steamAccountSystem = new SteamAccountSystem(attributes);
		}

		public Task Initialize() {
			if (!Packsize.Test())
				throw new VersionNotFoundException(
					"[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
			if (!DllCheck.Test())
				throw new DllNotFoundException(
					"[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");

			bool initialized = SteamAPI.Init();
			if (!initialized)
				throw new Exception(
					"[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");

			_steamAccountSystem.Initialize();

			CapabilityBuilder capabilityBuilder = new() {
				LocalSavingSystem = false,
				AccountSupport = true,
				PrimaryAccount = true,
				PrimaryAccountEstablishLimited = false,
				AdditionalAccountSystem = false,
				AccountInputPairingSystem = false,
				AccountName = true,
				AccountPicture = true,
				AccountSavingSystem = true,
				AccountAchievementSystem = true,
				AccountManualSignOut = false,
			};

			Capabilities = capabilityBuilder.ToCapabilities();

			return Task.CompletedTask;
		}
	}
}