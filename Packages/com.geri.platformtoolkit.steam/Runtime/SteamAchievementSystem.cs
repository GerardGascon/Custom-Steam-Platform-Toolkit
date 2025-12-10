using System;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamAchievementSystem : IAchievementSystem {
		public void Unlock(string id) {
			SteamUserStats.SetAchievement(id);
			SteamUserStats.StoreStats();
		}

		/// <summary>
		/// If the achievement and stat IDs are different, <paramref name="id"/> should use this format: ACH|STAT
		/// </summary>
		public void UpdateProgress(string id, int progress) {
			(string achievement, string stat) = ParseProgressiveAchievementID(id);

			uint max = GetMaxProgress(achievement);
			SteamUserStats.IndicateAchievementProgress(achievement, (uint)progress, max);
			SteamUserStats.SetStat(stat, (uint)progress);

			if (progress >= max)
				SteamUserStats.SetAchievement(achievement);

			SteamUserStats.StoreStats();
		}

		private static uint GetMaxProgress(string achievement) {
			bool limitsExist = SteamUserStats.GetAchievementProgressLimits(achievement, out int _, out int iMax);
			if (limitsExist)
				return (uint)iMax;

			limitsExist = SteamUserStats.GetAchievementProgressLimits(achievement, out float _, out float fMax);
			if (limitsExist)
				return (uint)fMax;

			throw new Exception($"Achievement '{achievement}' limit not found");
		}

		private static (string achievement, string stat) ParseProgressiveAchievementID(string id) {
			string[] parts = id.Split('|');
			if (parts.Length == 1)
				return (parts[0], parts[0]);
			return (parts[0], parts[1]);
		}
	}
}