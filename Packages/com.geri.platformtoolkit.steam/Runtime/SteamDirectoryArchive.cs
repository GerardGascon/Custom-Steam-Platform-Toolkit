using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.PlatformToolkit;

namespace Geri.PlatformToolkit.Steam {
	internal class SteamDirectoryArchive : AbstractArchive {
		private readonly SteamSaveFileHelper _saveFileHelper;
		private readonly string _root;
		private readonly bool _writeable;
		private readonly bool _exists = true;

		public SteamDirectoryArchive(SteamSaveFileHelper saveFileHelper, string root, bool writeable) {
			_saveFileHelper = saveFileHelper;
			_root = root;
			_writeable = writeable;

			if (_saveFileHelper.EnumerateDirectoriesInDirectory().Contains(root))
				return;

			if (!_writeable)
				throw new FileNotFoundException($"Folder '{root}' for read-only archive does not exist.");
			_exists = false;
		}

		public override Task<IReadOnlyList<string>> EnumerateFiles() {
			if (!_exists)
				return Task.FromResult<IReadOnlyList<string>>(new List<string>());
			return Task.FromResult(_saveFileHelper.EnumerateFilesInDirectory(_root));
		}

		protected override async Task<byte[]> GetDataFromStorage(string name) {
			return await _saveFileHelper.LoadDataFromFile(Path.Combine(_root, name));
		}

		protected override Task WriteDataToStorage(string name, byte[] data) {
			return Task.CompletedTask;
		}

		protected override async Task CommitDataToStorage(IReadOnlyCollection<(string name, byte[] data)> writtenFiles,
			IReadOnlyCollection<string> deletedFiles) {
			if (!_writeable)
				return;

			if (!_exists)
				_saveFileHelper.CreateDirectory(_root);

			foreach ((string name, byte[] data) in writtenFiles)
				await _saveFileHelper.WriteDataToFile(Path.Combine(_root, name), data);

			foreach (string deletedFile in deletedFiles)
				_saveFileHelper.RemoveFile(deletedFile);
		}

		protected override void RemoveDataFromStorage(string name) { }
	}
}