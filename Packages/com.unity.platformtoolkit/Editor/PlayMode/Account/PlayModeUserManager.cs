using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using Unity.Properties;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Class for managing user accounts in Edit and Play Mode.
    /// </summary>
    internal class PlayModeUserManager : IPlayModeUserManager, IDisposable
    {
        private PlayModeUserData m_SerializedData;

        private readonly GenericLifetimeToken m_LifetimeToken = new GenericLifetimeToken();

        private TaskCompletionSource<PlayModeAccountData> m_PendingEstablishAccountSource;
        private bool m_SignInAttemptedAndRefused = false;

        // We use this to forcibly set the primary account on entering playmode where we can confidently auto select a primary account
        private bool m_AutoAssignPrimaryMode = false;

        public IReadOnlyList<PlayModeAccountData> SignedInAccounts => m_SerializedData.SignedInAccounts;
        public IReadOnlyList<PlayModeAccountData> AccountData => m_SerializedData.AccountData;

        private IPlayModeCapability m_Capability;
        private INotificationManager m_NotificationManager;

        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the ScriptableObject that owns this object.
        private ScriptableObjectDataChangePersistor m_Persistor;

        private void OnAccountDataChanged()
        {
            AccountDataUpdateEvent?.Invoke();
        }

        public PlayModeAccountData PrimaryAccountData => m_SerializedData.PrimaryAccountData;

        public bool IsPickAccountRequestActive => m_PendingEstablishAccountSource != null;
        private IEnvironment m_Environment;

        public event Action PrimaryAccountChangeEvent;
        public event Action AccountDataUpdateEvent;
        public event Action PickAccountRequestReceivedEvent;
        public event Action PickAccountRequestResolvedEvent;
        public event Action<PlayModeAccountData, AccountState> AccountStateChangeEvent;

        private bool m_IsInPlayMode;

        public PlayModeUserManager(PlayModeUserData serializedData, ScriptableObjectDataChangePersistor persistor, IEnvironment environment, IPlayModeCapability playModeCapability, bool isInPlayMode)
        {
            m_SerializedData = serializedData ?? throw new ArgumentNullException(nameof(serializedData));

            m_Persistor = persistor ?? throw new ArgumentNullException(nameof(persistor));
            m_Persistor.OnDataChanged += OnAccountDataChanged;

            m_Capability = playModeCapability ?? throw new ArgumentNullException(nameof(playModeCapability));
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));

            m_NotificationManager = environment.NotificationManager ?? throw new ArgumentNullException(nameof(environment.NotificationManager));

            m_IsInPlayMode = isInPlayMode;

            SetCapability(playModeCapability);
            foreach (var accountData in m_SerializedData.AccountData)
            {
                accountData.Initialize(m_Persistor);
            }

            InitialiseAccountState();
        }

        public void Dispose()
        {
            m_LifetimeToken.TryAtomicDispose();

            if (m_Persistor != null)
            {
                m_Persistor.OnDataChanged -= OnAccountDataChanged;
            }

            m_Persistor = null;

            // Cancel any outstanding calls to Establish()
            m_Environment.NotificationManager.StopPendingEstablishUserNotification();
            m_PendingEstablishAccountSource?.SetException(new ObjectDisposedException(nameof(PlayModeUserManager)));
            m_PendingEstablishAccountSource = null;

            foreach (var accountData in m_SerializedData.AccountData)
            {
                accountData.Dispose();
            }
        }



        private void InitialiseAccountState()
        {
            if (m_IsInPlayMode)
            {
                if (m_Capability.PrimaryAccountBehaviour == PrimaryAccountBehaviour.AlwaysSignedIn && PrimaryAccountData == null)
                {
                    if (m_SerializedData.SignedInAccounts.Count != 1)
                        throw new InvalidOperationException("You cannot start the game without a primary account");

                    m_AutoAssignPrimaryMode = true;
                    SetToPrimary(m_SerializedData.SignedInAccounts[0]);
                    m_AutoAssignPrimaryMode = false;
                }
            }
            else
            {
                if (m_Capability.AdditionalAccountBehaviour == AdditionalAccountBehaviour.SignInOnGameRequestAndSignOutAnytime ||
                    m_Capability.AdditionalAccountBehaviour == AdditionalAccountBehaviour.NotSupported)
                {
                    foreach (var account in m_SerializedData.AccountData)
                    {
                        if ((account == PrimaryAccountData && m_Capability.PrimaryAccountBehaviour != PrimaryAccountBehaviour.NotSupported) ||
                            !m_SerializedData.SignedInAccounts.Contains(account))
                            continue;
                        SignOutAccount(account);
                    }
                }
            }
        }

        public void SignInAccount(PlayModeAccountData data)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            // This is not possible in the Play Mode Controls Window as this check is already performed
            var canSignIn = CanSignInAccount(data);
            if (canSignIn != SignInStatus.Allowed)
                throw new InvalidOperationException($"This user cannot sign-in in this state. SignInState: {canSignIn}");

            m_SerializedData.SignedInAccounts.Add(data);
            m_Persistor.PersistWrites();

            AccountStateChangeEvent?.Invoke(data, AccountState.SignedIn);

            //Every Platform that allows primary accounts cannot have non-primary accounts signed in, while the primary is not signed in
            //Therefore we automatically set the primary account when it has not been set
            if (m_SerializedData.PrimaryAccountData == null
            && CanSetAccountToPrimary(data) == SetPrimaryAccountStatus.Allowed
            && m_Capability.PrimaryAccountBehaviour != PrimaryAccountBehaviour.NotSupported)
            {
                SetToPrimary(data);
            }
        }

        public void SetToPrimary(PlayModeAccountData data)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (PrimaryAccountData == data)
                return;

            if (data != null)
            {
                var setPrimaryAccountStatus = CanSetAccountToPrimary(data);
                if (setPrimaryAccountStatus != SetPrimaryAccountStatus.Allowed)
                    throw new InvalidOperationException($"The primary account cannot be set in this state. SetPrimaryAccountStatus: {setPrimaryAccountStatus}");

                var canSignIn = CanSignInAccount(data);
                if (canSignIn == SignInStatus.Allowed)
                    SignInAccount(data);
                else if (canSignIn != SignInStatus.AlreadySignedIn)
                    throw new InvalidOperationException($"This account cannot be signed in, hence it cannot be set to primary. SignInState: {canSignIn}");
            }

            // Re-check if the account data has changed, as a sign-in may have recursively called SetToPrimary and we don't want duplicated events.
            if (PrimaryAccountData != data)
            {
                m_SerializedData.PrimaryAccountData = data;

                m_Persistor.PersistWrites();
                PrimaryAccountChangeEvent?.Invoke();
            }
        }

        public void SignOutAccount(PlayModeAccountData data)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            // This is not possible in the Play Mode Controls Window as this check is already performed
            var signOutStatus = CanSignOutAccount(data);
            if (signOutStatus != SignOutStatus.Allowed)
                throw new InvalidOperationException($"This user cannot sign-out in this state. SignOutState: {signOutStatus}");

            if (PrimaryAccountData == data)
            {
                if (m_IsInPlayMode)
                {
                    switch (m_Capability.PrimaryAccountBehaviour)
                    {
                        case PrimaryAccountBehaviour.AlwaysSignedIn:
                        case PrimaryAccountBehaviour.OptionalAndImmutable:
                            throw new InvalidOperationException("Primary account cannot be signed out on this Platform");
                        case PrimaryAccountBehaviour.OptionalAndMutable:
                            //The only platform with this capability is Xbox and this is how it functions, therefore we are replicating it
                            if (m_SerializedData.SignedInAccounts.Count > 1)
                                SetToPrimary(m_SerializedData.SignedInAccounts.Find(account => account != PrimaryAccountData));
                            else
                                SetToPrimary(null);
                            break;
                        case PrimaryAccountBehaviour.NotSupported:
                            break;
                    }
                }
                else
                {
                    SetToPrimary(null);
                }
            }

            m_SerializedData.SignedInAccounts.Remove(data);
            m_Persistor.PersistWrites();

            AccountStateChangeEvent?.Invoke(data, AccountState.SignedOut);
        }

        public bool IsAccountSignedIn(PlayModeAccountData data)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            return m_SerializedData.SignedInAccounts.Contains(data);
        }

        public void CreateInitialAccountSet()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            Texture2D[] photos = PlayModeControlsAssetTracker.GetInitialAccountPictures().ToArray();
            for (int i = 0; i < 4; i++)
            {
                CreateNewAccount();
                m_SerializedData.AccountData[i].PublicName = $"Test User {i + 1}";
                m_SerializedData.AccountData[i].PrivateName = $"Test User {i + 1} (Private)";
                m_SerializedData.AccountData[i].Picture = photos[i];
            }
        }

        public void CreateNewAccount()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            var accountData = new PlayModeAccountData();
            accountData.Initialize(m_Persistor);
            // Populate the account's starting achievements.
            // TODO: this should be done automatically when scriptable objects are created.
            accountData.Achievements.UpdateAchievementList();
            m_SerializedData.AccountData.Add(accountData);
            m_Persistor.PersistWrites();

            AccountDataUpdateEvent?.Invoke();
        }

        // TODO: Add tests for this once #587 adds PlayModeUserManagerTests.
        public void DeleteAccount(PlayModeAccountData account)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (IsAccountSignedIn(account))
                SignOutAccount(account);

            if (m_SerializedData.AccountData.Remove(account))
            {
                m_Persistor.PersistWrites();
                AccountDataUpdateEvent?.Invoke();
            }
            else
                throw new KeyNotFoundException($"Can't delete unknown account {account.PublicName}");
        }

        public void SetCapability(IPlayModeCapability capability)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (m_Capability == capability)
                return;

            m_Capability = capability;

            //TODO: This is a temporary solution, ideally we would only sign out what needs to sign out not everything.
            SetToPrimary(null);
            foreach(var account in AccountData)
            {
                if (IsAccountSignedIn(account))
                {
                    SignOutAccount(account);
                    AccountStateChangeEvent?.Invoke(account, AccountState.SignedOut);
                }
            }
        }

        public SignInStatus CanSignInAccount(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            CheckAccountData(accountData);

            if (IsAccountSignedIn(accountData))
                return SignInStatus.AlreadySignedIn;

            switch (m_Capability.AdditionalAccountBehaviour)
            {
                case AdditionalAccountBehaviour.NotSupported:
                {
                    if (m_Capability.PrimaryAccountBehaviour == PrimaryAccountBehaviour.NotSupported)
                        return SignInStatus.AdditionalAccountsNotSupported;

                    return m_SerializedData.SignedInAccounts.Count == 0 ? SignInStatus.Allowed : SignInStatus.AdditionalAccountsNotSupported;
                }
                case AdditionalAccountBehaviour.SignInAndOutAnytime:
                case AdditionalAccountBehaviour.SignInOnGameRequestAndSignOutAnytime:
                    return m_SerializedData.SignedInAccounts.Count < m_Capability.MaxSignedInAccounts ? SignInStatus.Allowed : SignInStatus.MaximumAccountsReached;
            }

            return SignInStatus.Allowed;
        }

        public SignInStatus CanSignInAccountManually(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            var canSignIn = CanSignInAccount(accountData);
            if (canSignIn != SignInStatus.Allowed)
                return canSignIn;

            if (m_Capability.AdditionalAccountBehaviour == AdditionalAccountBehaviour.SignInOnGameRequestAndSignOutAnytime)
            {
                // If we are not play mode and the primary account is not signed in, we can sign in manually
                if (m_Capability.PrimaryAccountBehaviour != PrimaryAccountBehaviour.NotSupported
                && !m_IsInPlayMode
                && m_SerializedData.PrimaryAccountData == null)
                {
                    return SignInStatus.Allowed;
                }

                return SignInStatus.OnlyAllowedWithRequests;
            }

            return SignInStatus.Allowed;
        }

        public SignOutStatus CanSignOutAccount(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            CheckAccountData(accountData);

            if (!m_SerializedData.SignedInAccounts.Contains(accountData))
                return SignOutStatus.NotSignedIn;

            if (PrimaryAccountData == accountData && m_IsInPlayMode
                && (m_Capability.PrimaryAccountBehaviour is PrimaryAccountBehaviour.AlwaysSignedIn or PrimaryAccountBehaviour.OptionalAndImmutable))
                return SignOutStatus.CannotSignOutPrimaryInPlayMode;

            return SignOutStatus.Allowed;
        }

        public SetPrimaryAccountStatus CanSetAccountToPrimary(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            CheckAccountData(accountData);

            switch (m_Capability.PrimaryAccountBehaviour)
            {
                case PrimaryAccountBehaviour.NotSupported:
                    return SetPrimaryAccountStatus.NotSupported;
                case PrimaryAccountBehaviour.AlwaysSignedIn:
                    return m_AutoAssignPrimaryMode || !m_IsInPlayMode ? SetPrimaryAccountStatus.Allowed : SetPrimaryAccountStatus.NotSupportedInPlaymode;
                case PrimaryAccountBehaviour.OptionalAndImmutable:
                    if (m_IsInPlayMode && (PrimaryAccountData != null))
                        return SetPrimaryAccountStatus.NotSupportedInPlaymode;
                    break;
            }

            //We auto sign-in the primary user therefore this check is necessary
            var canSignIn = CanSignInAccount(accountData);
            if (canSignIn != SignInStatus.Allowed && canSignIn != SignInStatus.AlreadySignedIn)
                return SetPrimaryAccountStatus.CannotSignIn;

            return SetPrimaryAccountStatus.Allowed;
        }

        public SetPrimaryAccountStatus CanSetAccountToPrimaryManually(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            CheckAccountData(accountData);

            var canSetPrimary = CanSetAccountToPrimary(accountData);
            if (canSetPrimary != SetPrimaryAccountStatus.Allowed)
                return canSetPrimary;

            //We auto sign-in the primary user therefore this check is necessary
            var canSignIn = CanSignInAccountManually(accountData);
            if (canSignIn != SignInStatus.Allowed && canSignIn != SignInStatus.AlreadySignedIn)
                return SetPrimaryAccountStatus.CannotSignIn;

            return SetPrimaryAccountStatus.Allowed;
        }

        private void CheckAccountData(PlayModeAccountData accountData)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (accountData == null)
                throw new ArgumentNullException("The given accountData is null");

            if (!m_SerializedData.AccountData.Contains(accountData))
                throw new InvalidOperationException("Account data asset does not exist");
        }

        public async Task<PlayModeAccountData> EstablishPrimaryAccount()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (IsPickAccountRequestActive)
                throw new InvalidOperationException("Can't establish primary account while the account picker is open");

            if (m_Capability.PrimaryAccountBehaviour is PrimaryAccountBehaviour.NotSupported)
                throw new InvalidOperationException("Establishing a primary account is not supported, make sure to check capabilities before calling!");

            if (!m_Capability.AllowMultipleSignInAttempts && m_SignInAttemptedAndRefused)
                throw new UserRefusalException("This platform refuses further sign in attempts when refused prior");

            if (m_SerializedData.SignedInAccounts.Count == 1 && PrimaryAccountData == null)
            {
                if (CanSetAccountToPrimary(m_SerializedData.SignedInAccounts[0]) == SetPrimaryAccountStatus.Allowed)
                    SetToPrimary(m_SerializedData.SignedInAccounts[0]);
            }

            if (PrimaryAccountData != null)
            {
                if (!IsAccountSignedIn(PrimaryAccountData))
                    throw new InvalidOperationException("Primary account is already set, but it somehow hasn't been signed in as required! Please make sure to sign in before setting.");

                // TODO: Forced refusal and pausing
                return PrimaryAccountData;
            }

            TaskCompletionSource<PlayModeAccountData> completionSource = new();
            m_PendingEstablishAccountSource = completionSource;

            NotifyPendingEstablish();

            try
            {
                var data = await completionSource.Task;

                switch (CanSetAccountToPrimary(data))
                {
                    case SetPrimaryAccountStatus.CannotSignIn:
                        throw new InvalidOperationException("We are attempting to set an account as primary, but the account is unable to sign in!");
                    case SetPrimaryAccountStatus.NotSupported:
                        throw new InvalidOperationException("We are attempting to set a signed in account as primary, but this platform does not support primary accounts!");
                    case SetPrimaryAccountStatus.NotSupportedInPlaymode:
                        throw new InvalidOperationException("We are attempting to set a signed in account as primary, but this platform does not allowing setting it in play mode!");
                }

                SetToPrimary(data);

                return data;
            }
            finally
            {
                PickAccountRequestResolvedEvent?.Invoke();
            }
        }

        public async Task<PlayModeAccountData> PickAccount()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (IsPickAccountRequestActive)
                throw new InvalidOperationException("Can't pick an account while the account picker is open");

            if (m_Capability.AdditionalAccountBehaviour is AdditionalAccountBehaviour.NotSupported)
                throw new InvalidOperationException("Picking an additional account is not supported, make sure to check capabilities before calling!");

            TaskCompletionSource<PlayModeAccountData> completionSource = new();
            m_PendingEstablishAccountSource = completionSource;

            NotifyPendingEstablish();

            try
            {
                return await completionSource.Task;
            }
            finally
            {
                PickAccountRequestResolvedEvent?.Invoke();
            }
        }

        public void OnAccountPickRefused()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (!IsPickAccountRequestActive)
                throw new InvalidOperationException("We are trying to refuse while there is no pending request to establish");

            m_SignInAttemptedAndRefused = true;

            ClearPossiblePendingRequestWithRefusal();
        }

        private void NotifyPendingEstablish()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            m_NotificationManager.StartPendingEstablishUserNotification();
            PickAccountRequestReceivedEvent?.Invoke();
        }

        private void ClearPossiblePendingRequestWithRefusal()
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            m_NotificationManager.StopPendingEstablishUserNotification();
            var completionSource = m_PendingEstablishAccountSource;
            m_PendingEstablishAccountSource = null;

            completionSource?.SetException(new UserRefusalException("The platform or user refused to establish an account"));
        }

        public void OnAccountPicked(PlayModeAccountData data)
        {
            m_LifetimeToken.ThrowOnDisposedAccess();

            if (!IsPickAccountRequestActive)
                throw new InvalidOperationException("We are trying to finish while there is no pending request to establish");

            CheckAccountData(data);

            var canSignIn = CanSignInAccount(data);
            if (canSignIn == SignInStatus.Allowed)
                SignInAccount(data);
            else if (canSignIn != SignInStatus.AlreadySignedIn)
                throw new InvalidOperationException($"We are trying to sign in an account that cannot be signed in! SignInStatus: {canSignIn}");

            m_NotificationManager.StopPendingEstablishUserNotification();
            var completionSource = m_PendingEstablishAccountSource;
            m_PendingEstablishAccountSource = null;

            completionSource.SetResult(data);
        }
    }
}
