using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal class TaskStop : IDisposable
    {
        private bool m_Disposed;
        private bool m_Stopped;
        private TaskCompletionSource<bool> m_StopCompletionSource;

        public bool Stopped => m_Stopped;

        public TaskStop(bool stopped)
        {
            m_Stopped = stopped;
        }

        public void Stop()
        {
            m_Stopped = true;
        }

        public void Continue()
        {
            m_Stopped = false;
            m_StopCompletionSource?.TrySetResult(true);
        }

        public async Task WhileStopped()
        {
            m_StopCompletionSource = new TaskCompletionSource<bool>();
            if (!m_Disposed && m_Stopped)
            {
                await m_StopCompletionSource.Task;
            }
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;
            m_StopCompletionSource?.TrySetResult(true);
        }
    }
}
