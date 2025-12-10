using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAchievementSystem : IAchievementSystem {
		public void Unlock(string id) {
			SteamUserStats.SetAchievement(id);
			SteamUserStats.StoreStats();
			SteamAPI.RunCallbacks();
		}

		public void UpdateProgress(string id, int progress) {
			SteamUserStats.GetAchievementProgressLimits(id, out int _, out int max);
			SteamUserStats.IndicateAchievementProgress(id, (uint)progress, (uint)max);
			SteamUserStats.SetStat(id, (uint)progress);

			if (progress >= max)
				SteamUserStats.SetAchievement(id);

			SteamUserStats.StoreStats();
			SteamAPI.RunCallbacks();
		}
	}
}