using System;
using System.Collections.Generic;

namespace Unity.PlatformToolkit
{
    internal interface IAccountModifier<TAccount> : IAsyncDisposable
    {
        /// <summary>Primary account at the beginning of modification. This value will not get updated by calling <see cref="Add"/>, <see cref="Remove"/> or <see cref="MakePrimary"/>.</summary>
        public TAccount CurrentPrimaryAccount { get; }

        /// <summary>Signed in accounts at the beginning of modification. This value will not get updated by calling <see cref="Add"/>, <see cref="Remove"/> or <see cref="MakePrimary"/>.</summary>
        public IReadOnlyList<TAccount> SignedInAccounts { get; }

        /// <summary>Add a new signed in account.</summary>
        /// <exception cref="InvalidOperationException">Attempting to add account that is already in the <see cref="SignedInAccounts"/>.</exception>
        /// <param name="account"></param>
        public void Add(TAccount account);

        /// <summary>Remove the account from the account system.</summary>
        /// <remarks>
        /// <para>When removing the primary account an event to make primary account null will be queued up.</para>
        /// <para>Will call the <see cref="IDoubleSignOut.TrySignOut"/> and if it returns true, will queue up a <see cref="IDoubleSignOut.CleanUpAfterSignOut"/> call.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Attempting to remove an account that is not in <see cref="SignedInAccounts"/>.</exception>
        /// <param name="account"></param>
        public void Remove(TAccount account);

        /// <summary>Mark account as primary.</summary>
        /// <exception cref="InvalidOperationException">Attempting to make primary an account that was removed or that was not added.</exception>
        /// <param name="account"></param>
        public void MakePrimary(TAccount account);
    }
}
