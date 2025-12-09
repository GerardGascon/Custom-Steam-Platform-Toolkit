using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAchievementSystem : AbstractAchievementSystem<AchievementDefinition> {
		public SteamAchievementSystem(IReadOnlyList<AchievementDefinition> achievements,
			int timeBetweenUpdatesMilliseconds = 100, ILifetimeToken parentLifetimeToken = null)
			: base(achievements, timeBetweenUpdatesMilliseconds, parentLifetimeToken) { }

		/// <summary>
		/// This method should check if there are any easy checks to see if the achievement system will fail to load.
		/// </summary>
		/// <returns>A Task that completes when initialization completes.</returns>
		protected override Task InitializeSystem() {
			return Task.CompletedTask;
		}

		/// <summary>
		/// Fetches the achievement data from the console, and maps it with PT Achievement data
		/// </summary>
		/// <returns></returns>
		protected override Task FetchNativeAchievementData() {
			//TODO: Ideally we should query the user from SteamAccount
			SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
			return Task.CompletedTask;
		}

		/// <summary>
		/// Platform achievement update event. This method must not throw any exceptions. If update fails, but can be retried,
		/// do not retry immediately.
		/// </summary>
		/// <param name="achievement">Achievement which progress to update.</param>
		/// <param name="progress">Progress to set. Progress is in PT achievement range.</param>
		/// <returns></returns>
		protected override Task DoUpdate(AchievementDefinition achievement, int progress) {
			if (achievement.Progressive) {
				SteamUserStats.IndicateAchievementProgress(achievement.Id, (uint)progress, (uint)achievement.ProgressTarget);
				SteamUserStats.SetStat(achievement.Id, (uint)progress);
			}

			if (progress >= achievement.ProgressTarget)
				SteamUserStats.SetAchievement(achievement.Id);

			SteamUserStats.StoreStats();
			return Task.CompletedTask;
		}
	}
}