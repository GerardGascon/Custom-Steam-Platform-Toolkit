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

			bool exists = await savingSystem.SaveExists("test");
			Assert.That(exists, Is.True);
		}

		[Test]
		public async Task SaveDataToFile_Subdirectory() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			ISaveWritable writable = await savingSystem.OpenSaveWritable("test/potato");

			await writable.WriteFile("file", System.Text.Encoding.UTF8.GetBytes("hello"));
			await writable.Commit();

			ISaveReadable readable = await savingSystem.OpenSaveReadable("test/potato");
			byte[] data = await readable.ReadFile("file");

			string text = System.Text.Encoding.UTF8.GetString(data);
			Assert.That(text, Is.EqualTo("hello"));

			bool exists = await savingSystem.SaveExists("test/potato");
			Assert.That(exists, Is.True);
		}

		[Test]
		public async Task DataStoreStoreInt() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			DataStore writeDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			writeDataStore.SetInt("cheese", 99);
			await writeDataStore.Save(savingSystem, "save-slot-1");

			DataStore readDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			Assert.That(readDataStore.GetInt("cheese"), Is.EqualTo(99));
		}

		[Test]
		public async Task DataStoreStoreString() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			DataStore writeDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			writeDataStore.SetString("alias", "Cool Rat");
			await writeDataStore.Save(savingSystem, "save-slot-1");

			DataStore readDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			Assert.That(readDataStore.GetString("alias"), Is.EqualTo("Cool Rat"));
		}

		[Test]
		public async Task DataStoreStoreFloat() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();
			DataStore writeDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			writeDataStore.SetFloat("cat-ratio", .56f);
			await writeDataStore.Save(savingSystem, "save-slot-1");

			DataStore readDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			Assert.That(readDataStore.GetFloat("cat-ratio"), Is.EqualTo(.56f));
		}

		[Test]
		public async Task DataStoreGetWithDefaultValue() {
			await PlatformToolkit.Initialize();

			ISavingSystem savingSystem = await PlatformToolkit.Accounts.Primary.Current.GetSavingSystem();

			DataStore readDataStore = await DataStore.Load(savingSystem, "save-slot-1");
			Assert.That(readDataStore.GetFloat("cat-ratio", .56f), Is.EqualTo(.56f));
		}
	}
}