using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace Geri.PlatformToolkit.Steam {
	public class SteamSaveFileHelper {
		private readonly string _path;

		public SteamSaveFileHelper(CSteamID accountId) {
			_path = Path.Combine(Application.persistentDataPath, "pt_saves", accountId.ToString());
			Directory.CreateDirectory(_path);
		}

		public string GetPath() => _path;

		public IReadOnlyList<string> EnumerateDirectoriesInDirectory() {
			return Directory.GetDirectories(_path);
		}

		public void CreateDirectory(string name) {
			Directory.CreateDirectory(Path.Combine(_path, name));
		}

		public void DeleteDirectory(string name) {
			Directory.Delete(Path.Combine(_path, name), true);
		}

		public IReadOnlyList<string> EnumerateFilesInDirectory(string name) {
			return Directory.GetFiles(Path.Combine(_path, name));
		}

		public async Task<byte[]> LoadDataFromFile(string name) {
			return await File.ReadAllBytesAsync(Path.Combine(_path, name));
		}

		public async Task WriteDataToFile(string name, byte[] data) {
			await  File.WriteAllBytesAsync(Path.Combine(_path, name), data);
		}

		public void RemoveFile(string deletedFile) {
			File.Delete(Path.Combine(_path, deletedFile));
		}
	}
}