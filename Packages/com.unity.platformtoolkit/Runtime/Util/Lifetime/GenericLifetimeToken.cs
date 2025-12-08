using System;

namespace Unity.PlatformToolkit
{
    internal enum DisposeReason
    {
        ExplicitDispose,
        InvalidAccount
    }

    /// <summary>
    /// ILifetimeToken which supports atomic dispose and setting dispose reason
    /// </summary>
    internal class GenericLifetimeToken : ILifetimeToken
    {
        private const string k_ExplicitDisposeReasonMessage = "Object is disposed";
        private const string k_InvalidAccountExceptionMessage = "Account is signed out";

        private AtomicFlag m_DisposedFlag;
        private DisposeReason m_Reason;

        /// <summary>
        /// Call when disposing of the token.
        /// </summary>
        /// <param name="reason">Reason dictates the type of the exception and message thrown when calling ThrowOnDisposedAccess.</param>
        /// <returns>True when called for the first time, false subsequently.</returns>
        public bool TryAtomicDispose(DisposeReason reason = DisposeReason.ExplicitDispose)
        {
            if (m_DisposedFlag.TestAndSet())
            {
                return false;
            }
            else
            {
                m_Reason = reason;
                return true;
            }
        }

        public bool Disposed => m_DisposedFlag.Value;

        public void ThrowOnDisposedAccess()
        {
            if(!m_DisposedFlag.Value)
                return;

            throw m_Reason switch
            {
                DisposeReason.ExplicitDispose => new ObjectDisposedException(k_ExplicitDisposeReasonMessage),
                DisposeReason.InvalidAccount => new InvalidAccountException(k_InvalidAccountExceptionMessage),
                _ => new ArgumentOutOfRangeException()
            };
        }
    }
}
