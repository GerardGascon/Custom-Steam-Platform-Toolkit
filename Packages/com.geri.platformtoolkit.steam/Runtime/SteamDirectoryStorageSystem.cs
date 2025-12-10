using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	// This storage system is only for querying directories, not files
	internal class SteamDirectoryStorageSystem : AbstractStorageSystem {
		private readonly SteamSaveFileHelper _saveFileHelper;

		public SteamDirectoryStorageSystem(CSteamID userID) {
			_saveFileHelper = new SteamSaveFileHelper(userID);
		}

		public override Task<IReadOnlyList<string>> EnumerateArchives() {
			return Task.FromResult(_saveFileHelper.EnumerateDirectoriesInDirectory());
		}
		public override Task DeleteArchive(string name) {
			_saveFileHelper.DeleteDirectory(name);
			return Task.CompletedTask;
		}

		public override Task<IGenericArchive> GetReadOnlyArchive(string name) {
			return Task.FromResult<IGenericArchive>(new SteamDirectoryArchive(_saveFileHelper, name, false));
		}

		public override Task<IGenericArchive> GetWriteOnlyArchive(string name) {
			return Task.FromResult<IGenericArchive>(new SteamDirectoryArchive(_saveFileHelper, name, true));
		}
	}
}