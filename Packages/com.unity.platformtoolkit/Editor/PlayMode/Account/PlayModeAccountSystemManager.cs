using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal sealed class PlayModeAccountSystemManager : IPrimaryAccountSystemProvider, IAccountPickerSystemProvider, IAsyncDisposable
    {
        public IAccountSystem AccountSystem => m_AccountSystem;

        private const string k_AccountPickCanceledDisposedExceptionMessage = "Cancelling account request: account system has been disposed. This is expected after exiting playmode while simulating long-running tasks.";

        private GenericAccountSystem<PlayModeAccount> m_AccountSystem;
        private ICapabilities m_Capabilities;
        private IEnvironment m_Environment;
        private IPlayModeUserManager m_UserManager;

        private bool m_GetPrimaryCalled;
        private bool m_Disposed;

        private Task m_RunningPrimaryAccountChangeProcessors = Task.CompletedTask;
        private Task m_RunningAccountChangeProcessor = Task.CompletedTask;

        public PlayModeAccountSystemManager(IEnvironment environment, IPlayModeUserManager userManager, ICapabilities capabilities)
        {
            m_Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            m_UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public void Initialize()
        {
            var initialAccounts = new List<PlayModeAccount>();
            PlayModeAccount primaryAccount = null;
            foreach (var accountData in m_UserManager.SignedInAccounts)
            {
                var playModeAccount = new PlayModeAccount(m_Environment, m_UserManager, accountData, m_Capabilities);
                initialAccounts.Add(playModeAccount);
                if (accountData == m_UserManager.PrimaryAccountData)
                    primaryAccount = playModeAccount;
            }

            int primaryAccountIndex = -1;
            IPrimaryAccountSystemProvider primaryAccountSystemProvider = null;
            if (m_Capabilities.PrimaryAccount)
            {
                primaryAccountSystemProvider = this;

                if (primaryAccount is not null)
                    primaryAccountIndex = initialAccounts.IndexOf(primaryAccount);
            }

            IAccountPickerSystemProvider accountPickerSystemProvider = null;
            if (m_Capabilities.AccountPicker)
                accountPickerSystemProvider = this;

            IInputOwnershipSystem inputOwnershipSystem = null;
#if INPUT_SYSTEM_AVAILABLE
            if (m_Capabilities.InputOwnership)
                inputOwnershipSystem = InputOwnership;
#endif //INPUT_SYSTEM_AVAILABLE
            m_AccountSystem = new GenericAccountSystem<PlayModeAccount>(initialAccounts: initialAccounts, primaryAccountIndex: primaryAccountIndex, primaryAccountSystemProvider: primaryAccountSystemProvider, accountPickerSystemProvider: accountPickerSystemProvider, inputOwnershipSystem: inputOwnershipSystem);

            // See comment on OnPlayModeStateChanged explaining why this should be removed.
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            m_UserManager.AccountStateChangeEvent += AccountStateChangeProcessor;
            m_UserManager.PrimaryAccountChangeEvent += PrimaryAccountChangeProcessor;
        }

        public async Task<IAccount> Establish()
        {
            if (!m_Capabilities.PrimaryAccount)
                throw new InvalidOperationException("Primary account is not supported on this platform.");

            if (m_Capabilities.PrimaryAccountEstablishLimited && m_GetPrimaryCalled)
                throw new InvalidOperationException("Establish can only be called once.");

            await m_Environment.WaitIfPaused();

            // Wait until after the pause to fail if offline
            if (m_Environment.OfflineNetwork)
                throw new TemporarilyUnavailableException("Cannot sign in whilst being offline");

            if (m_Disposed)
                throw new ObjectDisposedException(k_AccountPickCanceledDisposedExceptionMessage);

            if (m_AccountSystem.PrimaryCurrentUser is not null)
                return m_AccountSystem.PrimaryCurrentUser;

            var primaryAccountData = await m_UserManager.EstablishPrimaryAccount();
            var playModeAccount = GetAccountFromData(primaryAccountData);
            if (playModeAccount is null)
                throw new NullReferenceException("Unable to get a primary account");
            m_GetPrimaryCalled = true;
            return playModeAccount;
        }

        public async Task<IAccount> Show()
        {
            if (!m_Capabilities.AccountPicker)
                throw new InvalidOperationException("Account picker is not supported on this platform.");

            await m_Environment.WaitIfPaused();

            // Wait until after the pause to fail if offline
            if (m_Environment.OfflineNetwork)
                throw new TemporarilyUnavailableException("Cannot sign in whilst being offline");

            if (m_Disposed)
                throw new ObjectDisposedException(k_AccountPickCanceledDisposedExceptionMessage);

            var accountData = await m_UserManager.PickAccount();
            var account = GetAccountFromData(accountData);
            if (account is null)
                throw new NullReferenceException("Unable to get an account");

            return account;
        }

        // Workaround for how there's currently no end-play flow for the PlayModePlatformToolkit, but we need to immediately end account picker operations.
        private void OnPlayModeStateChanged(PlayModeStateChange newState)
        {
            if (newState != PlayModeStateChange.ExitingPlayMode)
                return;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            _ = DisposeAsync();
        }

        private void PrimaryAccountChangeProcessor()
        {
            if (m_Disposed) return;

            // Capturing the state of the Account Data at the time of the event.
            var primaryAccountData = m_UserManager.PrimaryAccountData;
            async Task ProcessPrimaryAccountChange()
            {
                await using var modifier = await m_AccountSystem.BeginAccountSystemModification();
                if (primaryAccountData is null)
                {
                    modifier.MakePrimary(null);
                    return;
                }

                // TODO: Add specific test during the PlayModeCoreAccountSystem unit test creation work to help catch regressions on this edge case (Unable to find the Account and throwing an exception)
                var primaryAccount = GetAccountFromData(primaryAccountData);
                modifier.MakePrimary(primaryAccount);
            }

            m_RunningPrimaryAccountChangeProcessors = Task.WhenAll(m_RunningPrimaryAccountChangeProcessors, ProcessPrimaryAccountChange());
        }

        private void AccountStateChangeProcessor(PlayModeAccountData accountData, AccountState accountState)
        {
            if (m_Disposed) return;
            async Task ProcessAccountChange()
            {
                await using var modifier = await m_AccountSystem.BeginAccountSystemModification();

                var account = GetAccountFromData(accountData);
                // New Account
                if (account is null)
                {
                    if (accountState != AccountState.SignedIn)
                        throw new InvalidOperationException("Recieved and account state change with an unexpected state for a new user, check that the user gets properly added");

                    account = new PlayModeAccount(m_Environment, m_UserManager, accountData, m_Capabilities);
                    modifier.Add(account);
                    return;
                }

                if (account.State == accountState)
                    throw new InvalidOperationException("Recieved an account state change when there was no different account state!");

                account.State = accountState;
                if (accountState is AccountState.SignedOut)
                    modifier.Remove(account);
            }

            m_RunningAccountChangeProcessor = Task.WhenAll(m_RunningAccountChangeProcessor, ProcessAccountChange());
        }

        internal PlayModeAccount GetAccountFromData(PlayModeAccountData accountData)
        {
            foreach (var account in m_AccountSystem.SignedInAccounts)
            {
                var playModeAccount = account as PlayModeAccount;
                if (playModeAccount.AccountData == accountData)
                    return playModeAccount;
            }
            return null;
        }

        public async ValueTask DisposeAsync()
        {
            m_Disposed = true;

            try
            {
                await m_RunningPrimaryAccountChangeProcessors;
                await m_RunningAccountChangeProcessor;
            }
            catch
            {
                // We are disposing of this class and the account system so we don't care
            }


            if (m_UserManager is not null)
            {
                m_UserManager.PrimaryAccountChangeEvent -= PrimaryAccountChangeProcessor;
                m_UserManager.AccountStateChangeEvent -= AccountStateChangeProcessor;
            }
#if INPUT_SYSTEM_AVAILABLE
            m_InputOwnershipSystem?.Dispose();
#endif // INPUT_SYSTEM_AVAILABLE
        }

#if INPUT_SYSTEM_AVAILABLE
        private PlayModeInputSystem m_InputSystem;
        private PlayModeInputOwnershipSystem m_InputOwnershipSystem;

        public void SetInputSystem(PlayModeInputSystem inputSystem)
        {
            m_InputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
        }

        public IInputOwnershipSystem InputOwnership
        {
            get
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(k_AccountPickCanceledDisposedExceptionMessage);

                if (!m_Capabilities.InputOwnership)
                    throw new InvalidOperationException("Account-input pairing is not supported on this platform.");

                if (m_InputSystem is null)
                    throw new InvalidOperationException("Input system is not set. Please call SetInputSystem() before accessing InputOwnership.");

                return m_InputOwnershipSystem ??= new PlayModeInputOwnershipSystem(m_InputSystem, this);
            }
        }
#endif // INPUT_SYSTEM_AVAILABLE
    }
}
