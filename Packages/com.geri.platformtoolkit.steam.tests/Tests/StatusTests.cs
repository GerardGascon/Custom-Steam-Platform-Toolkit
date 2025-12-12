using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PlatformToolkit;

namespace Tests {
	[TestFixture]
	public class StatusTests : SteamTests {
		[Test]
		public void SteamSDKInitializesProperly() {
			AsyncTestDelegate act = PlatformToolkit.Initialize;
			Assert.That(act, Throws.Nothing);
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
	}
}