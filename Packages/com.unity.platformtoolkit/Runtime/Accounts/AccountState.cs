namespace Unity.PlatformToolkit
{
    /// <summary>Indicates if an <see cref="IAccount"/> object is signed in or signed out.</summary>
    /// <seealso cref="IAccount.State"/>
    public enum AccountState {
        /// <summary>Indicates that <see cref="IAccount"/> is signed in.</summary>
        SignedIn,
        /// <summary>Indicates that <see cref="IAccount"/> is signed out.</summary>
        SignedOut
    }
}
