using NUnit.Framework;
using Steamworks;

namespace Tests {
	public class Tests {
		[Test]
		public void SteamSDKInitializesProperly() {
			Assert.IsTrue(SteamAPI.Init());
		}
	}
}