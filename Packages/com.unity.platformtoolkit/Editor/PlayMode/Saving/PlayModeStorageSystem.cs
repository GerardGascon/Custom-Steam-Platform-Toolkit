using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeStorageSystem : AbstractStorageSystem
    {
        private readonly IEnvironment m_Environment;
        private PlayModeSaveData m_SaveData;

        public PlayModeStorageSystem(IEnvironment environment, PlayModeSaveData saveData)
        {
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            m_SaveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
        }

        public override Task<IReadOnlyList<string>> EnumerateArchives()
        {
            IReadOnlyList<string> names = m_SaveData.GetSaveNames();
            return Task.FromResult(names);
        }

        public override Task<IGenericArchive> GetReadOnlyArchive(string name)
        {
            byte[] data;
            try
            {
                data = m_SaveData.ReadSave(name);
            }
            catch (KeyNotFoundException)
            {
                throw new FileNotFoundException($"Save {name} does not exist and can't be opened in read only mode");
            }

            var dataStream = new MemoryStream(data, false);
            return Task.FromResult<IGenericArchive>(new PlayModeSolidZipFileArchive(dataStream, name, false, m_SaveData.GetSaveInfo(name)));
        }

        public async override Task<IGenericArchive> GetWriteOnlyArchive(string name)
        {
            var dataStream = new MemoryStream();
            var saveInfo = new PlayModeSaveDataInfo();

            if (m_SaveData.ContainsSave(name))
            {
                await dataStream.WriteAsync(m_SaveData.ReadSave(name));
                saveInfo = m_SaveData.GetSaveInfo(name);
            }

            var archive = new PlayModeSolidZipFileArchive(dataStream, name, true, saveInfo);
            archive.OnCommit += name =>
            {
                if (m_Environment.FullStorage)
                    throw new IOException("There is not enough space on the disk");
                var dataToWrite = dataStream.ToArray();
                m_SaveData.WriteSave(name, dataToWrite, saveInfo);
                return Task.CompletedTask;
            };
            return archive;
        }

        public override Task DeleteArchive(string name)
        {
            m_SaveData.RemoveSave(name);
            return Task.CompletedTask;
        }
    }
}
