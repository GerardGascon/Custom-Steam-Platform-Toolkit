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
	}
}