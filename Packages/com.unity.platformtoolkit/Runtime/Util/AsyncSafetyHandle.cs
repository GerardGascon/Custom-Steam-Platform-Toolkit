using System;
using UnityEngine.Assertions;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// A lightweight type to help check for concurrent calls to awaitable APIs on the same system.
    /// </summary>
    internal class AsyncSafetyHandle
    {
        private AtomicFlag m_IsAcquired = new AtomicFlag();
        private string m_ErrorMessage;

        public AsyncSafetyHandle(string errorMessage)
        {
            Assert.IsNotNull(errorMessage);
            m_ErrorMessage = errorMessage;
        }

        public ScopedAcquisition Acquire()
        {
            return new ScopedAcquisition(this);
        }

        private void AcquireInternal()
        {
            if (m_IsAcquired.TestAndSet())
            {
                throw new InvalidOperationException(m_ErrorMessage);
            }
        }

        private void ClearInternal()
        {
            m_IsAcquired.Clear();
        }

        public struct ScopedAcquisition : IDisposable
        {
            private AsyncSafetyHandle m_Handle;

            public ScopedAcquisition(AsyncSafetyHandle handle)
            {
                m_Handle = handle;
                m_Handle.AcquireInternal();
            }

            public void Dispose()
            {
                m_Handle.ClearInternal();
            }
        }
    }
}
