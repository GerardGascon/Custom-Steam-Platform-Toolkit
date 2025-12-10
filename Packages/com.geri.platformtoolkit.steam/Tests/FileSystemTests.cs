using System.Threading.Tasks;
using NUnit.Framework;
using Unity.PlatformToolkit;

namespace Tests {
	[TestFixture]
	public class FileSystemTests : SteamTests {
		[Test]
		public async Task SaveDoesNotExistByDefault() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			bool exists = await savingSystem.SaveExists("test");

			Assert.That(exists, Is.False);
		}

		[Test]
		public async Task SaveDoesNotExistByDefault_Subdirectory() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			bool exists = await savingSystem.SaveExists("test/potato");

			Assert.That(exists, Is.False);
		}
	}
}