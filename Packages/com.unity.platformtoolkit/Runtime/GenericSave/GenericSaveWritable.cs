using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    /// <list type="bullet">
    /// <listheader><term>
    /// Provides following guarantees for <see cref="IGenericArchive"/> owned by this class:
    /// </term></listheader>
    /// <item><description>
    /// Access is linear, no calls to archive methods will overlap.
    /// </description></item>
    /// <item><description>
    /// Archive methods can return on any thread, this object will await the main thread.
    /// </description></item>
    /// <item><description>
    /// After archive is disposed no other calls on the archive object will be made. Including another dispose.
    /// </description></item>
    /// </list>
    internal class GenericSaveWritable : ISaveWritable, IInvalidationListener
    {
        private readonly AsyncLock m_Lock = new();
        private readonly string m_Name;
        private readonly IGenericArchive m_Archive;
        private readonly bool m_RunInBackground;

        private readonly GenericLifetimeToken m_LifetimeToken;
        private readonly ILifetimeToken m_CombinedLifetimeToken;
        private IDisposeListener m_DisposeListener;

        public GenericSaveWritable(IGenericArchive archive,
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

        public async Task WriteFile(string name, byte[] data)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken.ThrowOnDisposedAccess();

            SaveNameValidator.CheckFileName(name);

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            await m_Archive.WriteFile(name, data);
        }

        public async Task DeleteFile(string name)
        {
            using var lck = await m_Lock.LockAsync();
            m_CombinedLifetimeToken.ThrowOnDisposedAccess();

            SaveNameValidator.CheckFileName(name);

            if (m_RunInBackground)
                await Awaitable.BackgroundThreadAsync();

            await m_Archive.DeleteFile(name);
        }

        public async Task Commit()
        {
            using var lck = await m_Lock.LockAsync();
            try
            {
                m_CombinedLifetimeToken.ThrowOnDisposedAccess();
                if (m_RunInBackground)
                    await Awaitable.BackgroundThreadAsync();

                await m_Archive.Commit();
            }
            finally
            {
                await DisposeAsyncNoSemaphore();
            }
        }

        /// <summary>
        /// Call this method when disposing from within the semaphore lock block.
        /// This happens when disposing from within <see cref="Commit"/> for example.
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
