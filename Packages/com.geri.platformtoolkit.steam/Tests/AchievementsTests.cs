using System.Threading.Tasks;
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
	}
}