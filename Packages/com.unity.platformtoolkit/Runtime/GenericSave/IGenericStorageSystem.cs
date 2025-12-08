using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    internal interface IGenericStorageSystem : IAsyncDisposable
    {
        Task<IReadOnlyList<string>> EnumerateArchives();
        Task DeleteArchive(string name);
        Task<IGenericArchive> GetReadOnlyArchive(string name);
        Task<IGenericArchive> GetWriteOnlyArchive(string name);
        Task<bool> ArchiveExists(string name);
    }
}
