using System;
using System.Collections.Generic;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Manages save file content mutation before it's committed to the final save.
    /// </summary>
    internal class GenericSaveDataProvider
    {
        private object m_Lock = new();
        private readonly Dictionary<string, byte[]> m_WrittenData = new();
        private readonly HashSet<string> m_DeletedData = new();

        /// <summary>
        /// Writes the given data for the given name.
        /// </summary>
        /// <remarks>
        /// If data with the same name was previously deleted by calling <see cref="DeleteData"/>,
        /// writing will cancel deletion.
        /// </remarks>
        /// <param name="name">Name of the data.</param>
        /// <param name="data">Data to write.</param>
        /// <exception cref="ArgumentNullException">Name is null or empty.</exception>
        public void WriteData(string name, byte[] data)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            // Copy the data so future modifications aren't seen by this write.
            var dataCopy = (byte[])data.Clone();
            lock (m_Lock)
            {
                m_DeletedData.Remove(name);
                m_WrittenData[name] = dataCopy;
            }
        }

        /// <summary>
        /// Delete data.
        /// </summary>
        /// <remarks>
        /// If data with the same name was previously written by calling <see cref="WriteData"/>,
        /// deleting will discard the data.
        /// </remarks>
        /// <param name="name">Name of the data.</param>
        /// <exception cref="ArgumentNullException">Name is null or empty.</exception>
        public void DeleteData(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            lock (m_Lock)
            {
                m_WrittenData.Remove(name);
                m_DeletedData.Add(name);
            }
        }

        /// <summary>
        /// Get all written and deleted files.
        /// </summary>
        /// <remarks>
        /// A file cannot be both written and deleted, a file with a given name will only appear in one of the returned collections.
        /// </remarks>
        /// <param name="writtenFiles">Collection of written files: file names and written data.</param>
        /// <param name="deletedFiles">Collection of deleted file names.</param>
        public void GetModifiedFiles(out IReadOnlyCollection<(string name, byte[] data)> writtenFiles, out IReadOnlyCollection<string> deletedFiles)
        {
            lock (m_Lock)
            {
                var writtenFileList = new List<(string, byte[])>();
                foreach (var (key, value) in m_WrittenData)
                    writtenFileList.Add((key, value));
                writtenFiles = writtenFileList;

                var deletedFileList = new List<string>();
                foreach (var deletedFile in m_DeletedData)
                    deletedFileList.Add(deletedFile);
                deletedFiles = deletedFileList;
            }
        }
    }
}
