using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application = UnityEngine.Device.Application;

namespace Unity.PlatformToolkit
{
    internal class GenericLocalStorageSystem : AbstractStorageSystem
    {
        protected readonly string m_SavesPath = Path.Combine(Application.persistentDataPath, "pt_saves");

        private DriveInfo m_DriveInfo;

        private string GetPathForSave(string saveName) => Path.Combine(m_SavesPath, saveName);

        public GenericLocalStorageSystem(string pathOverride = null)
        {
            if (pathOverride != null)
                m_SavesPath = pathOverride;

            if (!Directory.Exists(m_SavesPath))
                Directory.CreateDirectory(m_SavesPath);
        }

        public override Task<IReadOnlyList<string>> EnumerateArchives()
        {
            var savePaths = Directory.GetFiles(m_SavesPath);

            var filteredCopy = new List<string>(savePaths.Length);
            foreach (var savePath in savePaths)
            {
                var filteredName = Path.GetFileName(savePath);
                if (SaveNameValidator.IsValidSaveName(filteredName))
                {
                    filteredCopy.Add(filteredName);
                }
            }
            return Task.FromResult<IReadOnlyList<string>>(filteredCopy);
        }

        public override Task<IGenericArchive> GetReadOnlyArchive(string name)
        {
            var data = File.ReadAllBytes(GetPathForSave(name));
            var dataStream = new MemoryStream(data, false);
            return Task.FromResult<IGenericArchive>(new SolidZipFileArchive(dataStream, name, false));
        }

        public override async Task<IGenericArchive> GetWriteOnlyArchive(string name)
        {
            var dataStream = new MemoryStream();

            var path = GetPathForSave(name);
            if (File.Exists(path))
                await dataStream.WriteAsync(File.ReadAllBytes(path));

            var archive = new SolidZipFileArchive(dataStream, name, true);
            archive.OnCommit += (string name) =>
            {
                var dataToWrite = dataStream.ToArray();
                ExceptionTesting.TriggerException(ExceptionPoint.PreCommit);
                File.WriteAllBytes(GetPathForSave(name), dataToWrite);
                return Task.CompletedTask;
            };
            return archive;
        }

        public override Task DeleteArchive(string name)
        {
            File.Delete(GetPathForSave(name));
            return Task.CompletedTask;
        }
    }
}
