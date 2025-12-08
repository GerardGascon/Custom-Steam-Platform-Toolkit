using System;
using System.Collections.Generic;
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
			//TODO: See what's this used for
			attributeDefinitions = new List<(string attributeId, Type attributeType, string attributeName)>()
				{ };
		}
	}
}