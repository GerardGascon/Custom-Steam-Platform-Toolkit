using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <summary>Save opened for writing.</summary>
    /// <remarks>
    /// <para><see cref="ISaveWritable"/> allows modifying multiple files within a save and committing them in a single operation. Changes to the save are first queued up by calling <see cref="WriteFile"/>, <see cref="DeleteFile"/>, <see cref="SetImage"/>, <see cref="Description"/>, then changes are commited by calling <see cref="Commit"/>. If <see cref="Commit"/> completes without exceptions, all queued up changes were written to disk successfully. If <see cref="Commit"/> throws an exception, no changes are made to existing data on disk.</para>
    /// <para>A save must be disposed before it can be opened again. If <see cref="ISaveWritable"/> is opened, it must be disposed before calling <see cref="ISavingSystem.OpenSaveReadable"/> or <see cref="ISavingSystem.OpenSaveWritable"/> with the same save name.</para>
    /// <para>Calling dispose discards any changes made to the save.</para>
    /// </remarks>
    /// <seealso cref="ISavingSystem"/>
    /// <seealso cref="ISavingSystem.OpenSaveWritable"/>
    public interface ISaveWritable : IAsyncDisposable, IDisposable
    {
        /// <summary>Write data into a file.</summary>
        /// <remarks>
        /// <para>If a file with a given name does not exist <see cref="WriteFile"/> will create the file.</para>
        /// <para>If a file with a given name already exists <see cref="WriteFile"/> will overwrite the entire file.</para>
        /// <para>If a file is deleted with <see cref="DeleteFile"/> and then written with <see cref="WriteFile"/>, the <see cref="DeleteFile"/> is canceled out.</para>
        /// </remarks>
        /// <param name="name">Name of the file.</param>
        /// <param name="data">Data to write.</param>
        /// <returns>Task representing the file writing operation.</returns>
        /// <exception cref="ArgumentException">The name is null or empty, contains invalid characters or is too long.</exception>
        /// <exception cref="CorruptedSaveException">The save has become corrupted, an empty save from a failed commit is considered corrupted.</exception>
        /// <exception cref="NotEnoughSpaceException">There is not enough space to write the data. This can mean that the system is out of memory, but other limits can also be imposed by platforms. For example limits on how much storage is allocated for each account or how large a single commit can be.</exception>
        /// <exception cref="System.IO.IOException">There was an error writing the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task WriteFile(string name, byte[] data);

        /// <summary>Delete a file.</summary>
        /// <remarks>
        /// <para>Attempts to delete a non-existent file are ignored.</para>
        /// <para>If a file is written with <see cref="WriteFile"/> and then deleted with <see cref="DeleteFile"/>, the <see cref="WriteFile"/> is canceled out and the file will be deleted or not created at all if it did not exist on disk.</para>
        /// </remarks>
        /// <param name="name">Name of the file.</param>
        /// <returns>Task representing the file deletion operation.</returns>
        /// <exception cref="ArgumentException">The name is null or empty, contains invalid characters or is too long.</exception>
        /// <exception cref="System.IO.IOException">There was an error deleting the data.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task DeleteFile(string name);

        /// <summary>Commit changes made to the save.</summary>
        /// <remarks>
        /// <para>If a file already exists in the save and was not modified, <see cref="Commit"/> will leave it as is.</para>
        /// <para>Commit disposes the <see cref="ISaveWritable"/>. This is true regardless if the commit succeeded or failed.</para>
        /// <para>When a commit fails with an exception no changes will be made to the save. Any changes waiting to be committed are discarded.</para>
        /// <para>Commit will fail if the save does not contain any files. This includes new empty saves, or existing save when attempting to delete all the files.</para>
        /// <para>If a commit is not completed succesfully when creating, the save may still be created but remain empty.</para>
        /// </remarks>
        /// <returns>Task representing the commit operation.</returns>
        /// <exception cref="System.IO.IOException">There was an error commiting the data.</exception>
        /// <exception cref="NotEnoughSpaceException">There is not enough space to write the data. This can mean that the system is out of memory, but other limits can also be imposed by platforms. For example limits on how much storage is allocated for each account or how large a single commit can be.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Save is empty</exception>
        Task Commit();
    }
}
