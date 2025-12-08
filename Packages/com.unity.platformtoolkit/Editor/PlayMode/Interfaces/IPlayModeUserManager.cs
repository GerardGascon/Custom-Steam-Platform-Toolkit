using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Properties;

namespace Unity.PlatformToolkit.PlayMode
{
    internal enum SetPrimaryAccountStatus
    {
        Allowed,
        CannotSignIn,
        NotSupported,
        NotSupportedInPlaymode
    }

    internal enum SignInStatus
    {
        Allowed,
        AlreadySignedIn,
        AdditionalAccountsNotSupported,
        MaximumAccountsReached,
        OnlyAllowedWithRequests
    }

    internal enum SignOutStatus
    {
        Allowed,
        NotSignedIn,
        CannotSignOutPrimaryInPlayMode
    }
    /// <summary>
    /// Unifies account management for the play mode controls system by tracking <see cref="PlayModeAccountData"/> and signed
    /// in accounts.
    /// </summary>
    internal interface IPlayModeUserManager
    {
        /// <summary>
        /// An event that fires whenever the <see cref="PrimaryAccountData"/> changes
        /// </summary>
        event Action PrimaryAccountChangeEvent;

        /// <summary>
        /// Primary <see cref="PlayModeAccountData"/> for the current environment configuration
        /// </summary>
        PlayModeAccountData PrimaryAccountData { get; }

        /// <summary>
        /// An event that fires whenever the <see cref="AccountData"/> is updated
        /// </summary>
        event Action AccountDataUpdateEvent;

        /// <summary>
        /// An enumerable <see cref="PlayModeAccountData"/> collection pulled from assets in the project
        /// </summary>
        IReadOnlyList<PlayModeAccountData> AccountData { get; }

        /// <summary>m_PendingEstablishAccountSource
        /// An enumerable <see cref="PlayModeAccountData"/> collection of accounts that have been signed-in in the play mode controls window
        /// </summary>
        IReadOnlyList<PlayModeAccountData> SignedInAccounts { get; }

        /// <summary>
        /// An event that fires whenever an account identifier has its <see cref="AccountState"/> changed
        /// </summary>
        event Action<PlayModeAccountData, AccountState> AccountStateChangeEvent;

        /// <summary>
        /// Signs in an account via a given <see cref="PlayModeAccountData"/>
        /// </summary>
        /// <param name="data"> The <see cref="PlayModeAccountData"/> to check against </param>
        void SignInAccount(PlayModeAccountData data);

        /// <summary>
        /// Signs out an account via a given <see cref="PlayModeAccountData"/>
        /// </summary>
        /// <param name="data">The <see cref="PlayModeAccountData"/> to check against </param>
        void SignOutAccount(PlayModeAccountData data);

        /// <summary>
        /// Checks if an account is signed in via a given <see cref="PlayModeAccountData"/>
        /// </summary>
        /// <param name="data"> The <see cref="PlayModeAccountData"/> to check against </param>
        /// <returns> A boolean that tells if an account is signed in, returns false if not </returns>
        bool IsAccountSignedIn(PlayModeAccountData data);

        /// <summary>
        /// Creates a new account (used in the account inspector UI).
        /// </summary>
        void CreateNewAccount();

        /// <summary>
        /// Creates 4 new accounts with names and photos for use in testing.
        /// </summary>
        void CreateInitialAccountSet();

        /// <summary>
        /// Deletes the specified account (used in the account inspector UI).
        /// </summary>
        /// <param name="account">The account to delete.</param>
        void DeleteAccount(PlayModeAccountData account);

        /// <summary>
        /// Checks if the given <see cref="PlayModeAccountData"/> can be signed in
        /// </summary>
        /// <param name="accountData">The <see cref="PlayModeAccountData"/> that will be checked</param>
        /// <returns> A <see cref="SignInStatus"/> that tells if an account is allowed to sign in or why it's not allowed </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the given accountData is null</exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given accountData asset file does not exist</exception>
        SignInStatus CanSignInAccount(PlayModeAccountData accountData);

        /// <summary>
        /// Checks if the given <see cref="PlayModeAccountData"/> can be signed in manually
        /// Mimics the behavior of platforms that only allow signing-in when the application triggers a sign in opportunity
        /// </summary>
        /// <param name="accountData">The <see cref="PlayModeAccountData"/> that will be checked </param>
        /// <returns> A <see cref="SignInStatus"/> that tells if an account is allowed to sign in or why it's not allowed </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the given accountData is null</exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given accountData asset file does not exist</exception>
        SignInStatus CanSignInAccountManually(PlayModeAccountData accountData);

        /// <summary>
        /// Checks if the given <see cref="PlayModeAccountData"/> can be signed out
        /// </summary>
        /// <param name="accountData">The <see cref="PlayModeAccountData"/> that will be checked</param>
        /// <returns> A <see cref="SignOutStatus"/>  that tells if the account is allowed to be signed out or why it's not allowed </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the given accountData is null</exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given accountData asset file does not exist</exception>
        SignOutStatus CanSignOutAccount(PlayModeAccountData accountData);

        /// <summary>
        /// Checks if the given <see cref="PlayModeAccountData"/> can be set as the primary account.
        /// </summary>
        /// <param name="accountData">The <see cref="PlayModeAccountData"/> that will be checked</param>
        /// <returns> A <see cref="SetPrimaryAccountStatus"/> that tells if the account is allowed to be set as the primary or why it's not allowed </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the given accountData is null</exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given accountData asset file does not exist</exception>
        SetPrimaryAccountStatus CanSetAccountToPrimary(PlayModeAccountData accountData);

        /// <summary>
        /// Checks if the given <see cref="PlayModeAccountData"/> can be set as the primary account manually using the UI.
        /// </summary>
        /// <param name="accountData">The <see cref="PlayModeAccountData"/> that will be checked</param>
        /// <returns> A <see cref="SetPrimaryAccountStatus"/> that tells if the account is allowed to be set as the primary manually or why it's not allowed </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the given accountData is null</exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given accountData asset file does not exist</exception>
        SetPrimaryAccountStatus CanSetAccountToPrimaryManually(PlayModeAccountData accountData);

        /// <summary>
        /// Sets the given <see cref="PlayModeAccountData"/> as the primary account.
        /// </summary>
        /// <param name="accountData"> The <see cref="PlayModeAccountData"/> that will be checked </param>
        /// <exception cref="InvalidOperationException"> Thrown when the given <paramref name="accountData"/> is not recognized </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the given <paramref name="accountData"/> can't be set as the primary due to environment capabilities </exception>
        public void SetToPrimary(PlayModeAccountData accountData);

        /// <summary>
        /// An event that fires whenever user input is required for resolving a <see cref="EstablishPrimaryAccount"/> or a <see cref="PickAccount"/>
        /// request from the <see cref="IPrimaryAccountSystem.Establish"/> or <see cref="IAccountPickerSystem.Show"/>.
        /// </summary>
        event Action PickAccountRequestReceivedEvent;

        /// <summary>
        /// An event that fires whenever a <see cref="PickAccountRequestReceivedEvent"/> has been resolved by a selection with <see cref="OnAccountPicked(PlayModeAccountData)"/>
        /// or a refusal by <see cref="OnAccountPickRefused"/> or exiting play mode
        /// </summary>
        event Action PickAccountRequestResolvedEvent;

        /// <summary>
        /// Whether an account picking request is active.
        /// </summary>
        /// <returns>Whether an account picking request is currently active.</returns>
        public bool IsPickAccountRequestActive => false;

        /// <summary>
        /// Establish a primary account for the <see cref="IPrimaryAccountSystem"/>.
        /// </summary>
        /// <returns> A <see cref="Task{PlayModeAccountData}"/> that completes once an account has been selected, or when the user has refused the request. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when there is already a pending request to pick an account </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the selected capability does not support primary </exception>
        /// <exception cref="UserRefusalException"> Thrown when <see cref="OnAccountPickRefused"/> refuses the request </exception>
        /// <exception cref="ObjectDisposedException"> Thrown when the <see cref="IPlayModeUserManager"/> has been disposed </exception>
        public Task<PlayModeAccountData> EstablishPrimaryAccount();

        /// <summary>
        /// Pick an account for the <see cref="IAccountPickerSystem"/>.
        /// </summary>
        /// <returns> A <see cref="Task{PlayModeAccountData}"/> that completes once an account has been selected, or when the user has refused the request. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when there is already a pending request to pick an account </exception>
        /// <exception cref="InvalidOperationException"> Thrown when the selected capability does not support additional accounts </exception>
        /// <exception cref="UserRefusalException"> Thrown when <see cref="OnAccountPickRefused"/> refuses the request </exception>
        /// <exception cref="ObjectDisposedException"> Thrown when the <see cref="IPlayModeUserManager"/> has been disposed </exception>
        public Task<PlayModeAccountData> PickAccount();

        /// <summary>
        /// Finishes an <see cref="EstablishPrimaryAccount"/> or a <see cref="PickAccount"/> request for an account and sets to the provided <see cref="PlayModeAccountData"/>.
        /// Requires that the account data provided already exists.
        /// </summary>
        /// <param name="data"> The account to be picked </param>
        /// <exception cref="InvalidOperationException"> Thrown when there isn't a pending <see cref="EstablishPrimaryAccount"/> or <see cref="PickAccount"/> request </exception>
        /// <exception cref="KeyNotFoundException"> Thrown when the provided <see cref="PlayModeAccountData"/> hasn't been registered </exception>
        /// <exception cref="ArgumentNullException"> Thrown when the provided <see cref="PlayModeAccountData"/> is null </exception>
        public void OnAccountPicked(PlayModeAccountData data);

        /// <summary>
        /// Refuses an <see cref="EstablishPrimaryAccount"/> or a <see cref="PickAccount"/> request
        /// </summary>
        /// <exception cref="InvalidOperationException"> Thrown when there isn't a pending request </exception>
        public void OnAccountPickRefused();
    }
}
