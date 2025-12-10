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

		[Test]
		public async Task SaveDataToFile() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			ISaveWritable writable = await savingSystem.OpenSaveWritable("test");

			await writable.WriteFile("file", System.Text.Encoding.UTF8.GetBytes("hello"));
			await writable.Commit();

			ISaveReadable readable = await savingSystem.OpenSaveReadable("test");
			byte[] data = await readable.ReadFile("file");

			string text = System.Text.Encoding.UTF8.GetString(data);
			Assert.That(text, Is.EqualTo("hello"));
		}
	}
}