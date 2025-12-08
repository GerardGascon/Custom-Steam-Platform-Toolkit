using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// An asynchronous lock class that provides "scoped" disposable locks that release on disposal.
    /// This is a less error-prone alternative to SemaphoreSlim, since it's harder to forget to Release() this lock.
    ///
    /// Sample use:
    /// class MyClass {
    ///     private readonly AsyncLock m_Lock = new();
    ///
    ///     async Task MyMethod() {
    ///         using (var lck = await m_Lock.LockAsync())
    ///         {
    ///             // Synchronized logic here.
    ///         }
    ///     }
    /// };
    /// </summary>
    internal class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim m_Semaphore = new SemaphoreSlim(1, 1);

        public void Dispose()
        {
            m_Semaphore.Dispose();
        }

        /// <summary>
        /// Wait to enter the lock.
        /// </summary>
        /// <returns>A disposable object that releases the lock on disposal.</returns>
        /// <exception cref="ObjectDisposedException">The lock has been disposed.</exception>
        public IDisposable Lock()
        {
            m_Semaphore.Wait();
            return new ScopedLock(m_Semaphore);
        }

        /// <summary>
        /// Asynchronously wait to enter the lock.
        /// </summary>
        /// <returns>A disposable object that releases the lock on disposal.</returns>
        /// <exception cref="ObjectDisposedException">The lock has been disposed.</exception>
        public async Task<IDisposable> LockAsync()
        {
            await m_Semaphore.WaitAsync();
            return new ScopedLock(m_Semaphore);
        }

        /// <summary>
        /// Disposable class that's returned by Lock and LockAsync, which releases the semaphore upon disposal.
        /// </summary>
        private class ScopedLock : IDisposable
        {
            private readonly SemaphoreSlim m_Semaphore;

            internal ScopedLock(SemaphoreSlim semaphore)
            {
                m_Semaphore = semaphore;
            }

            public void Dispose()
            {
                m_Semaphore.Release();
            }
        }
    }
}
