using Geri.PlatformToolkit.Steam;
using NUnit.Framework;
using Unity.PlatformToolkit;
using UnityEngine;

namespace Tests {
	[TestFixture]
	public class StatusTests {
		[SetUp]
		public void Initialize() {
			SteamRuntimeConfiguration config = ScriptableObject.CreateInstance<SteamRuntimeConfiguration>();
			PlatformToolkit.InjectImplementation(config.InstantiatePlatformToolkit());
		}

		[Test]
		public void SteamSDKInitializesProperly() {
			AsyncTestDelegate act = PlatformToolkit.Initialize;
			Assert.That(act, Throws.Nothing);
		}
	}
}