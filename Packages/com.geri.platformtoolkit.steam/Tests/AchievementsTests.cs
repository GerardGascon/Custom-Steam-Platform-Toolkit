using System.Threading.Tasks;
using NUnit.Framework;
using Steamworks;
using Unity.PlatformToolkit;

namespace Tests {
	[TestFixture]
	public class AchievementsTests : SteamTests {
		[Test]
		//TODO: Failing because steam api can't access user stats for some reason
		public async Task AchievementUnlock() {
			await PlatformToolkit.Initialize();

			IAchievementSystem achievementSystem = await PlatformToolkit.Accounts.Primary.Current.GetAchievementSystem();
			achievementSystem.Unlock("Winner");

			bool exists = SteamUserStats.GetAchievement("Winner", out bool unlocked);
			Assert.That(exists, Is.True);
			Assert.That(unlocked, Is.True);
		}
	}
}