using System.Threading.Tasks;
using Geri.PlatformToolkit.Steam;
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
	}
}