using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <list type="bullet">
    /// <listheader><term>
    /// Provides following guarantees for <see cref="IGenericStorageSystem"/> owned by this class:
    /// </term></listheader>
    /// <item><description>
    /// Access is linear, no calls to storage methods will overlap.
    /// </description></item>
    /// <item><description>
    /// Saves will be disposed before dispose is called on the storage object.
    /// </description></item>
    /// <item><description>
    /// Storage methods can return on any thread, this object will await the main thread.
    /// </description></item>
    /// <item><description>
    /// After storage is disposed no other calls on the storage object will be made.
    /// </description></item>
    /// </list>
    internal class GenericSavingSystem : ISavingSystem, IAsyncDisposable, IDisposeListener
    {
        /// <summary>
        /// Key - save name, Value - save, either GenericSaveReadable or GenericSaveWritable
        /// </summary>
        /// <remarks>
        /// While this dictionary is meant to prevents the same save from being opened twice. If another save system is set
        /// up to open the same saves, multiple opens are possible. Platform implementers beware. Can also happen if saves
        /// are opened outside of PT.
        /// </remarks>
        private readonly Dictionary<string, IInvalidationListener> m_OpenSaves =
            new Dictionary<string, IInvalidationListener>();

        private readonly AsyncLock m_Lock = new();

        private readonly IGenericStorageSystem m_Storage;
        private readonly bool m_RunInBackground;

        private readonly GenericLifetimeToken m_LifetimeToken;
        private readonly ILifetimeToken m_CombinedLifetimeToken;

        /// <param name="storage">
        /// Storage object implementing platform specific part of the saving system.
        /// </param>
        /// <param name="runInBackground">
        /// If true all async calls to storage object will be performed on the background thread. If false calling thread will not be enforced.
        /// </param>
        /// <param name="parentLifetimeToken">
        /// When this token becomes invalid, this object also will become invalid and all calls to it except dispose will throw.
        /// This object needs to be disposed even if the token is invalidated.
        /// </param>
        public GenericSavingSystem(IGenericStorageSystem storage, bool runInBackground = true,
            ILifetimeToken parentLifetimeToken = null)
        {
            m_Storage = storage;
            m_RunInBackground = runInBackground;
            m_LifetimeToken = new GenericLifetimeToken();
            m_CombinedLifetimeToken = new CombinedAnyLifetimeToken(parentLifetimeToken, m_LifetimeToken);
        }

        public async Task<IReadOnlyList<string>> EnumerateSaveNames()
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken?.ThrowOnDisposedAccess();

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            if (m_OpenSaves.Count != 0)
                throw new InvalidOperationException("Saves cannot be enumerated while some of them are open.");

            var result = await m_Storage.EnumerateArchives();
            return result;
        }

        public async Task<ISaveReadable> OpenSaveReadable(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken?.ThrowOnDisposedAccess();
            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            if (m_OpenSaves.ContainsKey(name))
            {
                throw new InvalidOperationException($"Could not open save {name} because it is already open.");
            }

            var archive = await m_Storage.GetReadOnlyArchive(name);
            var saveReadable = new GenericSaveReadable(archive, name, m_RunInBackground, m_CombinedLifetimeToken, this);
            m_OpenSaves[name] = saveReadable;

            return saveReadable;
        }

        public async Task<ISaveWritable> OpenSaveWritable(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken?.ThrowOnDisposedAccess();

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            if (m_OpenSaves.ContainsKey(name))
            {
                throw new InvalidOperationException($"Could not open save {name} because it is already open.");
            }

            var archive = await m_Storage.GetWriteOnlyArchive(name);
            var saveWritable = new GenericSaveWritable(archive, name, m_RunInBackground, m_CombinedLifetimeToken, this);

            m_OpenSaves[name] = saveWritable;
            return saveWritable;
        }

        public async Task<bool> SaveExists(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken?.ThrowOnDisposedAccess();

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            var result = await m_Storage.ArchiveExists(name);
            return result;
        }

        public async Task DeleteSave(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken?.ThrowOnDisposedAccess();

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            if (m_OpenSaves.ContainsKey(name))
            {
                throw new InvalidOperationException($"Could not delete save {name} because it is still open.");
            }

            await m_Storage.DeleteArchive(name);
        }

        public async Task OnAsyncDispose(string name)
        {
            using var lck = await m_Lock.LockAsync();
            if (!m_LifetimeToken.Disposed)
                m_OpenSaves.Remove(name, out _);
        }

        public void OnSyncDispose(string name)
        {
            using var lck = m_Lock.Lock();
            if (!m_LifetimeToken.Disposed)
                m_OpenSaves.Remove(name, out _);
        }

        /// <summary><para>
        /// Call this method when the entire saving system and all of its child objects need to be disposed,
        /// including all <see cref="ISaveReadable"/>, <see cref="ISaveWritable"/>, <see cref="IGenericStorageSystem"/>, and
        /// <see cref="IGenericArchive"/>. Do not try to dispose of all of these objects individually.
        /// </para><para>
        /// One situation where disposing of the entire saving system is required is when the saving system is owned by
        /// an account and the account becomes invalid.
        /// </para></summary>
        public async ValueTask DisposeAsync()
        {
            Dictionary<string, IInvalidationListener> openSavesCopy = null;
            using (var lck = await m_Lock.LockAsync())
            {
                if (m_LifetimeToken.TryAtomicDispose())
                {
                    // Make a copy of m_OpenSaves so it can be safely accessed without holding the semaphore,
                    // since listeners might try to remove themselves from this dictionary while we iterate over it.
                    openSavesCopy = new Dictionary<string, IInvalidationListener>(m_OpenSaves);
                    m_OpenSaves.Clear();
                }
            }

            if (openSavesCopy is not null)
            {
                foreach (var save in openSavesCopy.Values)
                {
                    await save.OnInvalidation();
                }
            }

            await m_Storage.DisposeAsync();
        }
    }
}
