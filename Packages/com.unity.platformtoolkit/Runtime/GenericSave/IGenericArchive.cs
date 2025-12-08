using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    internal interface IGenericArchive : IAsyncDisposable, IDisposable
    {
        string Name { get; }

        Task<IReadOnlyList<string>> EnumerateFiles();
        Task<byte[]> ReadFile(string name);
        Task WriteFile(string name, byte[] data);
        Task DeleteFile(string name);
        Task<bool> ContainsFile(string name);

        Task Commit();
    }
}
