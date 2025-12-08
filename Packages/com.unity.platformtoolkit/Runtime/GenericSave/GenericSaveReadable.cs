using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <seealso cref="GenericSaveWritable"/>
    internal class GenericSaveReadable : ISaveReadable, IInvalidationListener
    {
        private readonly AsyncLock m_Lock = new();
        private readonly string m_Name;
        private readonly IGenericArchive m_Archive;
        private readonly bool m_RunInBackground;

        private readonly GenericLifetimeToken m_LifetimeToken;
        private readonly ILifetimeToken m_CombinedLifetimeToken;
        private IDisposeListener m_DisposeListener;

        public GenericSaveReadable(IGenericArchive archive,
            string name,
            bool runInBackground = true,
            ILifetimeToken parentLifetimeToken = null,
            IDisposeListener disposeListener = null)
        {
            m_Name = name;
            m_Archive = archive ?? throw new ArgumentNullException(nameof(archive));
            m_RunInBackground = runInBackground;
            m_LifetimeToken = new GenericLifetimeToken();
            m_CombinedLifetimeToken = new CombinedAnyLifetimeToken(parentLifetimeToken, m_LifetimeToken);
            m_DisposeListener = disposeListener;
        }

        public async Task<IReadOnlyList<string>> EnumerateFiles()
        {
            using var lck = await m_Lock.LockAsync();

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            m_CombinedLifetimeToken.ThrowOnDisposedAccess();
            return await m_Archive.EnumerateFiles();
        }

        public async Task<byte[]> ReadFile(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken.ThrowOnDisposedAccess();

            SaveNameValidator.CheckFileName(name);

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            var data = await m_Archive.ReadFile(name);
            return data;
        }

        public async Task<bool> ContainsFile(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken.ThrowOnDisposedAccess();

            SaveNameValidator.CheckFileName(name);

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            return await m_Archive.ContainsFile(name);
        }

        /// <summary>
        /// Call this method when disposing from within the semaphore lock block.
        /// </summary>
        private async ValueTask DisposeAsyncNoSemaphore()
        {
            if (m_LifetimeToken.TryAtomicDispose())
            {
                try
                {
                    await m_Archive.DisposeAsync();
                }
                finally
                {
                    if (m_DisposeListener is not null)
                    {
                        await m_DisposeListener.OnAsyncDispose(m_Name);
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            using var lck = await m_Lock.LockAsync();
            await DisposeAsyncNoSemaphore();
        }

        public void Dispose()
        {
            using var lck = m_Lock.Lock();
            if (m_LifetimeToken.TryAtomicDispose())
            {
                try
                {
                    m_Archive.Dispose();
                }
                finally
                {
                    m_DisposeListener?.OnSyncDispose(m_Name);
                }
            }
        }

        public async Task OnInvalidation()
        {
            using var lck = await m_Lock.LockAsync();
            m_DisposeListener = null;
            await DisposeAsyncNoSemaphore();
        }
    }
}
