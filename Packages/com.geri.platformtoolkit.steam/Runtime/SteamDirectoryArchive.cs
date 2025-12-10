using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamDirectoryArchive : AbstractArchive {
		private readonly SteamSaveFileHelper _saveFileHelper;
		private readonly string _name;
		private readonly bool _writeable;
		private readonly bool _exists = true;

		public SteamDirectoryArchive(SteamSaveFileHelper saveFileHelper, string name, bool writeable) {
			_saveFileHelper = saveFileHelper;
			_name = name;
			_writeable = writeable;

			string fullPath = Path.Combine(_saveFileHelper.GetPath(), name);
			if (_saveFileHelper.EnumerateDirectoriesInDirectory().Contains(fullPath))
				return;

			if (!_writeable)
				throw new FileNotFoundException($"Folder '{fullPath}' for read-only archive does not exist.");
			_exists = false;
		}

		public override Task<IReadOnlyList<string>> EnumerateFiles() {
			if (!_exists)
				return Task.FromResult<IReadOnlyList<string>>(new List<string>());
			return Task.FromResult(_saveFileHelper.EnumerateFilesInDirectory(_name));
		}

		protected override async Task<byte[]> GetDataFromStorage(string name) {
			return await _saveFileHelper.LoadDataFromFile(name);
		}

		protected override Task WriteDataToStorage(string name, byte[] data) {
			return Task.CompletedTask;
		}

		protected override async Task CommitDataToStorage(IReadOnlyCollection<(string name, byte[] data)> writtenFiles,
			IReadOnlyCollection<string> deletedFiles) {
			if (!_writeable)
				return;

			if (!_exists)
				_saveFileHelper.CreateDirectory(_name);

			foreach ((string name, byte[] data) in writtenFiles)
				await _saveFileHelper.WriteDataToFile(name, data);

			foreach (string deletedFile in deletedFiles)
				_saveFileHelper.RemoveFile(deletedFile);
		}

		protected override void RemoveDataFromStorage(string name) { }
	}
}