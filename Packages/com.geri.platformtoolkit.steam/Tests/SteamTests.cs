using Geri.PlatformToolkit.Steam;
using NUnit.Framework;
using Steamworks;
using Unity.PlatformToolkit;
using UnityEngine;

namespace Tests {
	public abstract class SteamTests {
		[SetUp]
		public void Initialize() {
			SteamRuntimeConfiguration config = ScriptableObject.CreateInstance<SteamRuntimeConfiguration>();
			PlatformToolkit.InjectImplementation(config.InstantiatePlatformToolkit());
		}

		[TearDown]
		public void Cleanup() {
			SteamUserStats.ResetAllStats(true);
			SteamUserStats.StoreStats();
		}
	}
}