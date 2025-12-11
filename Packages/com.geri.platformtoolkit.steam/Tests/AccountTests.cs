using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Steamworks;
using Unity.PlatformToolkit;
using UnityEngine;

namespace Tests {
	[TestFixture]
	public class AccountTests : SteamTests {
		[Test]
		public async Task SteamPrimaryAccountNameIsProperlyReturned() {
			await PlatformToolkit.Initialize();

			string expectedName = SteamFriends.GetPersonaName();

			string name = await PlatformToolkit.Accounts.Primary.Current.GetName();
			Assert.That(name, Is.EqualTo(expectedName));
		}

		[Test]
		public async Task SteamAccountNameIsProperlyReturnedFromAccountList() {
			await PlatformToolkit.Initialize();

			string expectedName = SteamFriends.GetPersonaName();

			string name = await PlatformToolkit.Accounts.SignedIn[0].GetName();
			Assert.That(name, Is.EqualTo(expectedName));
		}

		[Test]
		public async Task SteamAccountPictureIsProperlyReturned() {
			await PlatformToolkit.Initialize();

			Texture2D picture = await PlatformToolkit.Accounts.Primary.Current.GetPicture();

			byte[] pngData = picture.EncodeToPNG();
			Assert.That(pngData, Is.Not.Null);

			string folderPath = Path.Combine(Application.dataPath, "SteamPlatformToolkitTestResults");
			string filePath = Path.Combine(folderPath, "ProfilePicture.png");

			if (!Directory.Exists(folderPath))
				Directory.CreateDirectory(folderPath);

			await File.WriteAllBytesAsync(filePath, pngData);

			Debug.Log($"Exported profile picture to: {filePath}");

			Assert.That(File.Exists(filePath), Is.True);
		}

		[Test]
		public async Task SteamAccountStateAttribute() {
			await PlatformToolkit.Initialize();

			EPersonaState expectedState = SteamFriends.GetPersonaState();

			EPersonaState state = await PlatformToolkit.Accounts.Primary.Current.GetAttribute<EPersonaState>("PERSONA_STATE");

			Assert.That(state, Is.EqualTo(expectedState));
		}

		[Test]
		public async Task SteamNicknameAttribute() {
			await PlatformToolkit.Initialize();

			string expectedNick = SteamFriends.GetPlayerNickname(SteamUser.GetSteamID());

			string nick = await PlatformToolkit.Accounts.Primary.Current.GetAttribute<string>("NICKNAME");

			Assert.That(nick, Is.EqualTo(expectedNick));
		}

		[Test]
		public async Task SteamUserIDAttribute() {
			await PlatformToolkit.Initialize();

			CSteamID expectedID = SteamUser.GetSteamID();

			CSteamID id = await PlatformToolkit.Accounts.Primary.Current.GetAttribute<CSteamID>("USERID");

			Assert.That(id, Is.EqualTo(expectedID));
		}

		[Test]
		public async Task SteamLevelAttribute() {
			await PlatformToolkit.Initialize();

			int expectedLevel = SteamFriends.GetFriendSteamLevel(SteamUser.GetSteamID());

			int level = await PlatformToolkit.Accounts.Primary.Current.GetAttribute<int>("STEAM_LEVEL");

			Assert.That(level, Is.EqualTo(expectedLevel));
		}

		[Test]
		public async Task SteamFriendCountAttribute() {
			await PlatformToolkit.Initialize();

			int expectedCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

			int count = await PlatformToolkit.Accounts.Primary.Current.GetAttribute<int>("FRIEND_COUNT");

			Assert.That(count, Is.EqualTo(expectedCount));
		}
	}
}