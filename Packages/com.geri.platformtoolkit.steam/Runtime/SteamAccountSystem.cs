using System;
using System.Collections.Generic;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAccountSystem : IAccountSystem {
		public IReadOnlyList<IAccount> SignedIn { get; private set; }
		public event Action<IAccount, AccountState> OnChange;
		private SteamPrimaryAccountSystem _steamAccountSystem;
		public IPrimaryAccountSystem Primary => _steamAccountSystem;

		public void Initialize() {
			_steamAccountSystem = new SteamPrimaryAccountSystem(new SteamAccount(SteamUser.GetSteamID()));
			SignedIn = new List<IAccount> {
				Primary.Current
			};
		}
	}
}