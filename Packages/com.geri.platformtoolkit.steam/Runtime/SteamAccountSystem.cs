using System;
using System.Collections.Generic;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAccountSystem : IAccountSystem {
		public IReadOnlyList<IAccount> SignedIn { get; private set; }
		public event Action<IAccount, AccountState> OnChange;

		public void Initialize() {
			SignedIn = new List<IAccount> {
				new SteamAccount(SteamUser.GetSteamID())
			};
		}
	}
}