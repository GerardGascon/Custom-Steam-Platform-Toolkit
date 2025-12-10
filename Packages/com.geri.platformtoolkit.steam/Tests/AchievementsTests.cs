using System.Threading.Tasks;
using Geri.PlatformToolkit.Steam;
using NUnit.Framework;
using Steamworks;
using Unity.PlatformToolkit;

namespace Tests {
	[TestFixture]
	public class AchievementsTests : SteamTests {
		[Test]
		public async Task AchievementUnlock() {
			await PlatformToolkit.Initialize();

			IAchievementSystem achievementSystem = await PlatformToolkit.Accounts.Primary.Current.GetAchievementSystem();
			achievementSystem.Unlock("ACH_WIN_ONE_GAME");

			bool exists = SteamUserStats.GetAchievement("ACH_WIN_ONE_GAME", out bool unlocked);
			Assert.That(exists, Is.True);
			Assert.That(unlocked, Is.True);
		}

		[Test]
		public async Task ProgressiveAchievementUpdate() {
			await PlatformToolkit.Initialize();

			IAchievementSystem achievementSystem = await PlatformToolkit.Accounts.Primary.Current.GetAchievementSystem();
			achievementSystem.UpdateProgress("ACH_TRAVEL_FAR_ACCUM|FeetTraveled", 100);

			SteamUserStats.GetStat("FeetTraveled", out float progress);
			Assert.That(progress, Is.EqualTo(100));
		}
	}
}