using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    abstract internal class AbstractStorageSystem : IGenericStorageSystem
    {
        public abstract Task<IReadOnlyList<string>> EnumerateArchives();
        public abstract Task<IGenericArchive> GetReadOnlyArchive(string name);
        public abstract Task<IGenericArchive> GetWriteOnlyArchive(string name);
        public abstract Task DeleteArchive(string name);

        public async Task<bool> ArchiveExists(string name)
        {
            var archives = await EnumerateArchives();
            return archives.Contains(name);
        }

        public virtual ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
