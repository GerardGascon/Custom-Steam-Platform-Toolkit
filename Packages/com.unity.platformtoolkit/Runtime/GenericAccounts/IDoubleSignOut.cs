using System.Threading.Tasks;

namespace Unity.PlatformToolkit
{
    /// <summary>Account that is signed out in two steps. First, the account is marked as signed out by calling <see cref="TrySignOut"/> and then <see cref="CleanUpAfterSignOut"/> is called to clean up any internal resources, like saving system or achievement system.</summary>
    internal interface IDoubleSignOut
    {
        /// <summary>Mark object as signed out.</summary>
        /// <remarks>
        /// <para>This method is called in a time critical environment, so it should not block.</para>
        /// <para>Use this method to set the <see cref="IAccount.State"/> to <see cref="AccountState.SignedOut"/> and to dispose of the <see cref="ILifetimeToken"/>.</para>
        /// </remarks>
        /// <returns>Returns true the first time the method is called on an object, returns false on every subsequent call.</returns>
        public bool TrySignOut();

        /// <summary>Dispose of any dependencies.</summary>
        /// <remarks>
        /// <para>This method can take its sweet time to dispose of any dependencies, nothing important should get blocked.</para>
        /// <para>Use this method to dispose of all dependencies like the <see cref="GenericSavingSystem"/>.</para>
        /// </remarks>
        /// <returns>Task representing the account clean up operation.</returns>
        public Task CleanUpAfterSignOut();
    }
}
