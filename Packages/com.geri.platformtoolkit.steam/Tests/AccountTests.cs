using System.Threading.Tasks;
using Geri.PlatformToolkit.Steam;
using NUnit.Framework;
using Steamworks;
using Unity.PlatformToolkit;
using UnityEngine;

namespace Tests {
	[TestFixture]
	public class AccountTests {
		[SetUp]
		public void Initialize() {
			SteamRuntimeConfiguration config = ScriptableObject.CreateInstance<SteamRuntimeConfiguration>();
			PlatformToolkit.InjectImplementation(config.InstantiatePlatformToolkit());
		}

		[Test]
		public async Task CapabilitiesAreProperlyInitialized() {
			await PlatformToolkit.Initialize();

			Assert.That(PlatformToolkit.Capabilities.Accounts, Is.True);
			Assert.That(PlatformToolkit.Capabilities.PrimaryAccount, Is.True);
			Assert.That(PlatformToolkit.Capabilities.AccountPicker, Is.False);
			Assert.That(PlatformToolkit.Capabilities.InputOwnership, Is.False);
			Assert.That(PlatformToolkit.Capabilities.PrimaryAccountEstablishLimited, Is.False);
			Assert.That(PlatformToolkit.Capabilities.AccountSaving, Is.True);
			Assert.That(PlatformToolkit.Capabilities.AccountAchievements, Is.True);
			Assert.That(PlatformToolkit.Capabilities.AccountManualSignOut, Is.False);
			Assert.That(PlatformToolkit.Capabilities.LocalSaving, Is.False);
		}

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