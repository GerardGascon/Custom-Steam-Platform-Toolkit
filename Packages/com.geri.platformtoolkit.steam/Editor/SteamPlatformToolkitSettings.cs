using System;
using System.Collections.Generic;
using Steamworks;
using Unity.PlatformToolkit.Editor;
using UnityEngine;

namespace Geri.PlatformToolkit.Steam.Editor {
	[Serializable]
	internal class SteamPlatformToolkitSettings : ISteamPlatformToolkitSettings {
		[SerializeField] private SteamAttributeSettings attributeSettings = new();

		public IAttributeSettings Attributes => attributeSettings;

		public SteamAttributeSettings SteamAttributeSettings => attributeSettings;
	}

	[Serializable]
	internal class SteamAttributeSettings : AttributeSettings {
		protected override void InitializeAttributes(
			out IReadOnlyList<(string AttributeId, Type AttributeType, string AttributeName)> attributeDefinitions) {
			attributeDefinitions = new List<(string attributeId, Type attributeType, string attributeName)> {
				("PersonaState", typeof(EPersonaState), "Persona State"),
				("Nickname", typeof(string), "Nickname"),
				("UserID", typeof(CSteamID), "User ID"),
				("SteamLevel", typeof(int), "Steam Level"),
				("FriendCount", typeof(int), "Friend Count"),
			};
		}
	}
}