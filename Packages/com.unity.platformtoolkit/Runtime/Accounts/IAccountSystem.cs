using System;
using System.Collections.Generic;

namespace Unity.PlatformToolkit
{
    /// <summary>Provides access to all <see cref="IAccount"/> related functionality.</summary>
    public interface IAccountSystem
    {
        /// <summary>Snapshot of currently signed in accounts.</summary>
        /// <remarks>The returned list will not be modified after it is returned. If you keep an instance of this list, it will get outdated, if accounts sign in or sign out.</remarks>
        IReadOnlyList<IAccount> SignedIn { get; }

        /// <summary>Invoked after a new account signs in and after a signed in account signs out. Event provides an <see cref="IAccount"/> that changed and <see cref="AccountState"/> showing the new state of the account.</summary>
        /// <remarks><see cref="AccountState.SignedIn"/> event is invoked after the account is added to <see cref="SignedIn"/>. <see cref="AccountState.SignedOut"/> event is invoked after the account is removed from <see cref="SignedIn"/>.</remarks>
        event Action<IAccount, AccountState> OnChange;

        /// <summary>Get the <see cref="IPrimaryAccountSystem"/>.</summary>
        /// <exception cref="InvalidOperationException">Thrown if the primary account is not supported on the current platform. Use <see cref="ICapabilities.PrimaryAccount"/> to check if the primary account is supported.</exception>
        IPrimaryAccountSystem Primary => throw new InvalidOperationException("Primary account is not supported on this platform.");

        /// <summary>Get the <see cref="IAccountPickerSystem"/>.</summary>
        /// <exception cref="InvalidOperationException">Thrown if account picker is not supported on the current platform. Use <see cref="ICapabilities.AccountPicker"/> to check if account picker is supported.</exception>
        IAccountPickerSystem Picker => throw new InvalidOperationException("Account picker is not supported on this platform.");

        /// <summary>Get the <see cref="IInputOwnershipSystem"/>.</summary>
        /// <exception cref="InvalidOperationException">Thrown if account-input pairing is not supported on the current platform. Use <see cref="ICapabilities.InputOwnership"/> to check if account-input pairing is supported.</exception>
        IInputOwnershipSystem InputOwnership => throw new InvalidOperationException("Account-input pairing is not supported on this platform.");
    }
}
