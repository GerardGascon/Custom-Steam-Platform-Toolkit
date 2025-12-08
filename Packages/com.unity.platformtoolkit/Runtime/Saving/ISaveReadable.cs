using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Save opened for reading.</summary>
    /// <remarks>A save must be disposed before it can be opened again. If <see cref="ISaveReadable"/> is opened, it must be disposed before calling <see cref="ISavingSystem.OpenSaveReadable"/> or <see cref="ISavingSystem.OpenSaveWritable"/> with the same save name.</remarks>
    /// <seealso cref="ISavingSystem"/>
    /// <seealso cref="ISavingSystem.OpenSaveReadable"/>
    public interface ISaveReadable : IAsyncDisposable, IDisposable
    {
        /// <summary>Enumerate files within the save.</summary>
        /// <returns>Task containing an IReadOnlyList of file name strings.</returns>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task<IReadOnlyList<string>> EnumerateFiles();

        /// <summary>Returns data read from the given file.</summary>
        /// <param name="name">The name of the file to load</param>
        /// <returns>Task containing a non-null array of data read from the file.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Data for name is not found.</exception>
        /// <exception cref="CorruptedSaveException">The save has become corrupted, an empty save from a failed commit is considered corrupted.</exception>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task<byte[]> ReadFile(string name);

        /// <summary>Checks if a file with the given name exists in the save.</summary>
        /// <param name="name">The name of the file.</param>
        /// <returns>Task containing a boolean indicating whether the file exists (true) or not (false).</returns>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task<bool> ContainsFile(string name);
    }
}
