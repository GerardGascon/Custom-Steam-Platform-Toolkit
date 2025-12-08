namespace Unity.PlatformToolkit
{
    /// <summary>
    /// ILifetimeToken defines a type which will throw an exception once it is disposed.
    /// Used to signal to objects that a dependency is disposed and provides an exception to throw on disposed access.
    /// </summary>
    internal interface ILifetimeToken
    {
        /// <summary>
        /// Determines whether this token is disposed.
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Throws an appropriate exception when called after token is disposed.
        /// </summary>
        void ThrowOnDisposedAccess();
    }
}
