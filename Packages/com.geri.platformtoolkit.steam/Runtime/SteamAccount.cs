using System;
using System.Threading.Tasks;
using Steamworks;
using Unity.PlatformToolkit;
using UnityEngine;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAccount : IAccount {
		private readonly CSteamID _userID;
		private readonly SteamAchievementSystem _achievementSystem;
		private GenericSavingSystem _savingSystem;
		private readonly SteamDirectoryStorageSystem _directoryStorageSystem;
		private readonly AccountAttributeProvider<SteamAccount> _accountAttributeProvider;

		//TODO: Manage this
		public AccountState State { get; }

		public SteamAccount(CSteamID userID, AccountAttributeProvider<SteamAccount> accountAttributeProvider) {
			_userID = userID;
			_accountAttributeProvider = accountAttributeProvider;
			_directoryStorageSystem = new SteamDirectoryStorageSystem(_userID);
			_achievementSystem = new SteamAchievementSystem();
		}

		public Task<bool> SignOut() {
			throw new InvalidOperationException("Sign Out is not supported on Steam.");
		}

		public Task<string> GetName() {
			return Task.FromResult(SteamFriends.GetPersonaName());
		}

		private static void FlipVertically(byte[] data, int width, int height) {
			int rowSize = width * 4;
			byte[] rowBuffer = new byte[rowSize];

			for (int y = 0; y < height / 2; y++) {
				int topRow = y * rowSize;
				int bottomRow = (height - y - 1) * rowSize;

				Buffer.BlockCopy(data, topRow, rowBuffer, 0, rowSize);
				Buffer.BlockCopy(data, bottomRow, data, topRow, rowSize);
				Buffer.BlockCopy(rowBuffer, 0, data, bottomRow, rowSize);
			}
		}

		public Task<Texture2D> GetPicture() {
			int avatar = SteamFriends.GetLargeFriendAvatar(_userID);
			if (!SteamUtils.GetImageSize(avatar, out uint width, out uint height))
				throw new InvalidAccountException("Get profile picture reference failed.");

			byte[] image = new byte[width * height * 4];
			if (!SteamUtils.GetImageRGBA(avatar, image, image.Length))
				throw new InvalidAccountException("Get profile picture data failed.");

			FlipVertically(image, (int)width, (int)height);

			Texture2D result = new((int)width, (int)height, TextureFormat.RGBA32, false, true);
			result.LoadRawTextureData(image);
			result.Apply();
			return Task.FromResult(result);
		}

		public bool HasAttribute<T>(string attributeName) {
			return _accountAttributeProvider.HasAttribute<T>(attributeName);
		}

		public Task<T> GetAttribute<T>(string attributeName) {
			return _accountAttributeProvider.GetAttribute<T>(this, attributeName);
		}

		public Task<ISavingSystem> GetSavingSystem() {
			_savingSystem ??= new GenericSavingSystem(_directoryStorageSystem);
			return Task.FromResult<ISavingSystem>(_savingSystem);
		}

		public Task<IAchievementSystem> GetAchievementSystem() {
			if (_achievementSystem == null)
				throw new InvalidOperationException("Achievement System not initialized.");

			return Task.FromResult<IAchievementSystem>(_achievementSystem);
		}

		public static Task<EPersonaState> GetPersonaStateAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult(EPersonaState.k_EPersonaStateAway);
		}

		public static Task<string> GetNicknameAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult("");
		}

		public static Task<CSteamID> GetUserIDAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult(CSteamID.Nil);
		}

		public static Task<int> GetSteamLevelAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult(0);
		}

		public static Task<int> GetFollowerCountAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult(0);
		}

		public static Task<int> GetFriendCountAttribute(SteamAccount arg) {
			// TODO: Properly implement this
			return Task.FromResult(0);
		}
	}
}