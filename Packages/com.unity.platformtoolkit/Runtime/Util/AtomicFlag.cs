using System.Threading;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Atomically set and clear a bool value
    /// </summary>
    internal struct AtomicFlag
    {
        public bool Value => m_Value == 1;

        private int m_Value;

        /// <summary>
        /// Sets flag to true, returns initial flag value
        /// </summary>
        public bool TestAndSet()
        {
            return Interlocked.CompareExchange(ref m_Value, 1, 0) == 1;
        }

        /// <summary>
        /// Sets flag to false
        /// </summary>
        public void Clear()
        {
            m_Value = 0;
        }
    }
}
