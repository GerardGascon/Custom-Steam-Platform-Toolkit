using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Helper class for importing and exporting save data to and from the play mode controls system. Useful for local debugging of the save system.
    /// </summary>
    internal class PlayModeImportExportSave
    {
        private readonly PlayModeSaveData m_Data;

        public PlayModeImportExportSave(PlayModeSaveData saveData)
        {
            m_Data = saveData;
        }

        public async Task AddNewSaveFromDisk(string fullPath)
        {
            // We need to do GetFullPath because we get the path like so: somePath/Folder/, this will make it somePath\Folder\ required for the split
            fullPath = Path.GetFullPath(fullPath);
            var saveName = Path.GetFileNameWithoutExtension(fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Last());
            SaveNameValidator.CheckSaveName(saveName);

            var data = await File.ReadAllBytesAsync(fullPath);
            MemoryStream stream = new MemoryStream(data);
            SolidZipFileArchive archive = new(stream, saveName, false);
            await CheckFileNames(archive);

            PlayModeSaveDataInfo info = await CreatePlayModeSaveDataInfo(archive, saveName);

            m_Data.WriteSave(saveName, data, info);
        }

        private Task<PlayModeSaveDataInfo> CreatePlayModeSaveDataInfo(SolidZipFileArchive archive, string saveName)
        {
            var result = new PlayModeSaveDataInfo();
            result.Name = saveName;

            return Task.FromResult(result);
        }

        public async Task ImportFilesToReplaceSaveData(string saveName, string filePath)
        {
            SaveNameValidator.CheckSaveName(saveName);

            var data = await File.ReadAllBytesAsync(filePath);
            MemoryStream stream = new MemoryStream(data);
            SolidZipFileArchive archive = new(stream, saveName, false);
            await CheckFileNames(archive);

            PlayModeSaveDataInfo info = await CreatePlayModeSaveDataInfo(archive, saveName);

            m_Data.WriteSave(saveName, data, info);
        }

        private async Task CheckFileNames(SolidZipFileArchive archive)
        {
            foreach (var name in await archive.EnumerateFiles())
            {
                SaveNameValidator.CheckFileName(name);
            }
        }

        public async Task ExportFilesFromSave(string saveName, string containingFolder)
        {
            await File.WriteAllBytesAsync(Path.Combine(containingFolder, saveName + ".zip"), m_Data.ReadSave(saveName));
        }
    }
}
