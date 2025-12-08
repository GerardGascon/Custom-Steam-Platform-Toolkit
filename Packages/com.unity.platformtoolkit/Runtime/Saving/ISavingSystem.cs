using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    /// <summary>Provides access to saving functionality.</summary>
    /// <remarks>
    /// <para>Saves are identified by name. The name is a string consisting of lowercase latin alphabet letters [a-z], numbers [0-9] and hyphens -. Platforms have different maximum name length restrictions, so it’s recommended to keep the name short.</para>
    /// <para>A save contains a set of files within it. Each file has a unique name within a save. The same naming rules apply to files as to saves.</para>
    /// <para>If <see cref="ISavingSystem"/> operations fail with <see cref="InvalidSystemException"/> that means that the saving system needs to be re-created by calling <see cref="IAccount.GetSavingSystem"/> or <see cref="PlatformToolkit.LocalSaving"/>. Exactly if and when a saving system can become invalid is platform dependent. Usually <see cref="InvalidSystemException"/> indicates that saves were changed from outside the application or even on another system.</para>
    /// </remarks>
    public interface ISavingSystem
    {
        /// <summary>Enumerate names of existing saves.</summary>
        /// <remarks>Saves can only be enumerated when no saves are open. Attempting to enumerate saves while one or more saves are open will result in <see cref="System.InvalidOperationException"/>.</remarks>
        /// <returns>Task containing save names in an IReadOnlyList.</returns>
        /// <exception cref="System.InvalidOperationException">One or more saves are open, which prevents them from being enumerated.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task<IReadOnlyList<string>> EnumerateSaveNames();

        /// <summary>Open a save in read-only mode.</summary>
        /// <remarks>
        /// <para>Within an <see cref="ISavingSystem"/> only one save with a given name can be opened at one time. This is true regardless if the save was opened using <see cref="OpenSaveReadable"/> or <see cref="OpenSaveWritable"/>. Multiple saves with different names can be open at the same time, though different platforms can have different limits on how many saves can be opened.</para>
        /// <para>Only saves that already exist can be opened in read-only mode.</para>
        /// </remarks>
        /// <param name="name">Name of the save to be opened.</param>
        /// <returns>Task containing the save.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="System.InvalidOperationException">Save is already open.</exception>
        /// <exception cref="System.ArgumentException">The name is null or empty, contains invalid characters, or is too long.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Save with a given name does not exist in the <see cref="ISavingSystem"/>.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while accessing the file.</exception>
        Task<ISaveReadable> OpenSaveReadable(string name);

        /// <summary>Open a save in write-only mode.</summary>
        /// <remarks>
        /// <para>Within an <see cref="ISavingSystem"/> only one save with a given name can be opened at one time. This is true regardless if the save was opened using <see cref="OpenSaveReadable"/> or <see cref="OpenSaveWritable"/>. Multiple saves with different names can be open at the same time, though different platforms can have different limits on how many saves can be opened.</para>
        /// <para>If the method is given a name of an existing save, that save is opened for writing.</para>
        /// <para>To create a new save call <see cref="OpenSaveWritable"/> and give it a new name. A new save is only created after it is successfully committed.</para>
        /// </remarks>
        /// <param name="name">Name of the save to be opened.</param>
        /// <returns>Task containing the save.</returns>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="System.InvalidOperationException">Save is already open.</exception>
        /// <exception cref="System.ArgumentException">The name is null or empty, contains invalid characters, or is too long.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while accessing the file.</exception>
        Task<ISaveWritable> OpenSaveWritable(string name);

        /// <summary>Check if a save exists.</summary>
        /// <remarks><see cref="SaveExists"/> can only be called when no saves are open.</remarks>
        /// <param name="name">Name of the save to be checked.</param>
        /// <returns>Task containing a bool representing whether the save exists.</returns>
        /// <exception cref="System.InvalidOperationException">One or more saves are open.</exception>
        /// <exception cref="System.IO.IOException">There was an error reading the data.</exception>
        /// <exception cref="System.ArgumentException">The name is null or empty, contains invalid characters, or is too long.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task<bool> SaveExists(string name);

        /// <summary>Delete save.</summary>
        /// <remarks>
        /// <para>The save must be closed when attempting to delete it.</para>
        /// <para>Attempting to delete a save that does not exist is allowed and will not result in an exception.</para>
        /// </remarks>
        /// <param name="name">Name of the save to be deleted.</param>
        /// <returns>Task representing the deletion operation.</returns>
        /// <exception cref="System.IO.IOException">There was an error deleting the save.</exception>
        /// <exception cref="System.InvalidOperationException">The save is open.</exception>
        /// <exception cref="System.ArgumentException">The name is null or empty, contains invalid characters, or is too long.</exception>
        /// <exception cref="InvalidAccountException"><see cref="IAccount"/> is signed out.</exception>
        /// <exception cref="InvalidSystemException"><see cref="ISavingSystem"/> is invalid.</exception>
        Task DeleteSave(string name);
    }
}
