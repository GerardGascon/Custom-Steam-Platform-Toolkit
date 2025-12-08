using System;
using System.Collections.Generic;
using System.Linq;
using Unity.PlatformToolkit;
using Unity.PlatformToolkit.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Geri.PlatformToolkit.Steam.Editor {
	internal class SteamBuilder : IPlatformToolkitBuilder {
		private readonly IReadOnlyCollection<BuildTarget> _supportedBuildTargets;
		private readonly SteamPlatformToolkitSettings _settings;

		public SteamBuilder(SteamPlatformToolkitSettings settings, IReadOnlyCollection<BuildTarget> buildTargets) {
			_supportedBuildTargets = buildTargets;
			_settings = settings;
		}

		public void PostBuild(BuildReport buildReport) { }

		BaseRuntimeConfiguration IPlatformToolkitBuilder.PrepareBuild(BuildReport buildReport) {
			if (!_supportedBuildTargets.Contains(buildReport.summary.platform))
				throw new InvalidOperationException(
					$"Attempting to build custom Steam implementation on an unsupported target '{buildReport.summary.platform}'");

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
			var runtimeConfig = ScriptableObject.CreateInstance<SteamRuntimeConfiguration>();
			runtimeConfig.attributes = _settings.SteamAttributeSettings.BuildAttributes();
			return runtimeConfig;
#else
			Debug.LogWarning(
				$"{nameof(IPlatformToolkitBuilder.PrepareBuild)} was called when UNITY_STANDALONE_WIN or UNITY_STANDALONE_LINUX or UNITY_STANDALONE_OSX is not defined, this will result in a fail to initialize the implementation.");
			return null;
#endif
		}
	}
}