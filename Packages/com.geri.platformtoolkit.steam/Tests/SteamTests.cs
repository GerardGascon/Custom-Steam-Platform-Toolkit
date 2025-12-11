using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			Dictionary<string, string> attributes = new() {
				{ "PersonaState", "PERSONA_STATE" },
				{ "Nickname", "NICKNAME" },
				{ "UserID", "USERID" },
				{ "SteamLevel", "STEAM_LEVEL" },
				{ "FollowerCount", "FOLLOWER_COUNT" },
				{ "FriendCount", "FRIEND_COUNT" },
			};
			config.attributes = new AttributeStore(attributes.Keys.ToList(), attributes.Values.ToList());

			PlatformToolkit.InjectImplementation(config.InstantiatePlatformToolkit());
		}

		[TearDown]
		public void Cleanup() {
			SteamUserStats.ResetAllStats(true);
			SteamUserStats.StoreStats();

			SteamSaveFileHelper fileHelper = new(SteamUser.GetSteamID());
			DirectoryInfo di = new(fileHelper.GetPath());

			Array.ForEach(di.GetFiles(), f => f.Delete());
			Array.ForEach(di.GetDirectories(), f => f.Delete(true));
		}
	}
}