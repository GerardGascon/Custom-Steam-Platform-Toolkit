using System;
using System.Collections.Generic;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAccountSystem : IAccountSystem {
		public IReadOnlyList<IAccount> SignedIn { get; private set; }
		public event Action<IAccount, AccountState> OnChange;
		private SteamPrimaryAccountSystem _steamAccountSystem;

		private readonly AccountAttributeProvider<SteamAccount> _accountAttributeProvider;

		public SteamAccountSystem(AttributeStore attributes) {
			_accountAttributeProvider = new AccountAttributeProvider<SteamAccount>(attributes.Attributes);
			_accountAttributeProvider.RegisterAttributeGetter("PersonaState", SteamAccount.GetPersonaStateAttribute);
			_accountAttributeProvider.RegisterAttributeGetter("Nickname", SteamAccount.GetNicknameAttribute);
			_accountAttributeProvider.RegisterAttributeGetter("UserID", SteamAccount.GetUserIDAttribute);
			_accountAttributeProvider.RegisterAttributeGetter("SteamLevel", SteamAccount.GetSteamLevelAttribute);
			_accountAttributeProvider.RegisterAttributeGetter("FriendCount", SteamAccount.GetFriendCountAttribute);
			_accountAttributeProvider.FinalizeRegistration();
		}

		public IPrimaryAccountSystem Primary => _steamAccountSystem;

		public void Initialize() {
			_steamAccountSystem = new SteamPrimaryAccountSystem(new SteamAccount(SteamUser.GetSteamID(), _accountAttributeProvider));
			SignedIn = new List<IAccount> {
				Primary.Current
			};
		}
	}
}