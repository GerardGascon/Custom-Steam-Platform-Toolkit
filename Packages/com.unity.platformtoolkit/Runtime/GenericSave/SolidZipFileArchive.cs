using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    internal class SolidZipFileArchive : AbstractArchive
    {
        public event Func<string, Task> OnCommit;

        private ZipArchive m_Archive;
        private Stream m_Stream;
        private readonly bool m_Writable;

        public SolidZipFileArchive(Stream stream, string name, bool writable)
        {
            Name = name;
            m_Writable = writable;
            m_Stream = stream;

            var mode = m_Writable ? ZipArchiveMode.Update : ZipArchiveMode.Read;
            m_Archive = new(m_Stream, mode);
        }

        public override Task<IReadOnlyList<string>> EnumerateFiles()
        {
            List<string> result = new();
            foreach (var entry in m_Archive.Entries)
            {
                result.Add(entry.Name);
            }
            return Task.FromResult<IReadOnlyList<string>>(result);
        }

        protected override async Task<byte[]> GetDataFromStorage(string name)
        {
            var entry = m_Archive.GetEntry(name);
            if (entry == null)
            {
                throw new FileNotFoundException($"Could not get data for file {name} in archive {Name}.");
            }

            var size = entry.Length;
            await using var stream = entry.Open();
            var data = new byte[size];
            var read = stream.Read(data);
            if (read != size)
            {
                throw new IOException($"Didn't read enough bytes for file {name} in archive {Name}");
            }
            return data;
        }

        protected override async Task WriteDataToStorage(string name, byte[] data)
        {
            var entry = m_Archive.GetEntry(name) ?? m_Archive.CreateEntry(name);
            await using var stream = entry.Open();
            stream.Write(data);
        }

        protected override void RemoveDataFromStorage(string name)
        {
            var entry = m_Archive.GetEntry(name);
            entry?.Delete();
        }

        protected override async Task CommitDataToStorage(IReadOnlyCollection<(string name, byte[] data)> writtenFiles, IReadOnlyCollection<string> deletedFiles)
        {
            m_Archive.Dispose();
            m_Archive = null;
            m_Stream.Close();
            await OnCommit.Invoke(Name);
        }

        public override void Dispose()
        {
            m_Archive?.Dispose();
            m_Stream.Dispose();
            m_Archive = null;
            m_Stream = null;
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            m_Archive?.Dispose();
            await m_Stream.DisposeAsync();
            m_Archive = null;
            m_Stream = null;
            await base.DisposeAsync();
        }
    }
}
