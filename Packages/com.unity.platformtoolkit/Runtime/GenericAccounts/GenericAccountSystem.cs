using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.PlatformToolkit
{
    internal partial class GenericAccountSystem<TAccount> : IAccountSystem where TAccount : class, IAccount, IDoubleSignOut
    {
        public IReadOnlyList<IAccount> SignedIn => m_SignedInAccounts;
        private ReadOnlyCollection<IAccount> m_SignedInAccounts;
        private readonly HashSet<TAccount> m_Accounts = new HashSet<TAccount>();

        public event Action<IAccount, AccountState> OnChange;

        public IPrimaryAccountSystem Primary
        {
            get
            {
                if (m_PrimaryAccountSystem == null)
                    throw new InvalidOperationException("Primary account system is not supported.");
                return m_PrimaryAccountSystem;
            }
        }
        private readonly GenericPrimaryAccountSystem m_PrimaryAccountSystem;

        public IAccountPickerSystem Picker
        {
            get
            {
                if (m_AccountPickerSystem == null)
                    throw new InvalidOperationException("Account picker system is not supported.");
                return m_AccountPickerSystem;
            }
        }
        private readonly GenericAccountPickerSystem m_AccountPickerSystem;

        private readonly IInputOwnershipSystem m_InputOwnershipSystem;
        public IInputOwnershipSystem InputOwnership
        {
            get
            {
                if (m_InputOwnershipSystem == null)
                    throw new InvalidOperationException("Input ownership system system is not supported.");
                return m_InputOwnershipSystem;
            }
        }

        internal IAccount PrimaryCurrentUser => m_PrimaryAccountSystem.CurrentPrimaryAccount;
        internal IReadOnlyCollection<IAccount> SignedInAccounts => m_SignedInAccounts;


        private AccountSystemModifier m_CurrentAccountModifier;
        private readonly Queue<TaskCompletionSource<IAccountModifier<TAccount>>> m_PendingAccountModifications = new Queue<TaskCompletionSource<IAccountModifier<TAccount>>>();

        /// <param name="initialAccounts">List of accounts that are signed in during initialization. These accounts will appear in <see cref="IAccountSystem.SignedIn"/>, but <see cref="IAccountSystem.OnChange"/> will not be called.</param>
        /// <param name="primaryAccountSystemProvider">Null if the <see cref="IPrimaryAccountSystem"/> is not supported, or provider instance if it is.</param>
        /// <param name="primaryAccountIndex">Index to <see cref="initialAccounts"/> list to make one of the accounts primary during the initialization. Set to -1 to leave primary account null. <see cref="IPrimaryAccountSystem.OnChange"/> will not be called for the initial primary account.</param>
        /// <param name="accountPickerSystemProvider">Null if the <see cref="IAccountPickerSystem"/> is not supported, or provider instance if it is.</param>
        /// <param name="inputOwnershipSystem">Null if <see cref="IInputOwnershipSystem"/> is not supported, instance if it is.</param>
        /// <code>
        /// // Sample implementation
        /// internal class SampleAccountSystemManager : IPrimaryAccountSystemProvider
        /// {
        ///     private GenericAccountSystem&lt;PlatformAccount&gt; m_AccountSystem;
        ///     public IAccountSystem AccountSystem => m_AccountSystem;
        ///
        ///     public SampleAccountSystemManager()
        ///     {
        ///         m_AccountSystem = new GenericAccountSystem&lt;PlatformAccount&gt;(
        ///             primaryAccountSystemProvider: this);
        ///
        ///         PlatformSDK.OnAccountSignIn += OnSignIn;
        ///         PlatformSDK.OnAccountSignOut += OnSignOut;
        ///     }
        ///
        ///     public async Task&lt;IAccount&gt; Establish()
        ///     {
        ///         await using var modifier = await m_AccountSystem.BeginAccountSystemModification().ConfigureAwait(false);
        ///         var currentPrimaryAccount = await PlatformSDK.GetPrimaryAccount();
        ///         if (!modifier.SignedInAccounts.Contains(currentPrimaryAccount))
        ///             modifier.Add(currentPrimaryAccount);
        ///         modifier.MakePrimary(currentPrimaryAccount);
        ///         return currentPrimaryAccount;
        ///     }
        ///
        ///     private async void OnSignIn(PlatformAccount account)
        ///     {
        ///         await using var modifier = await m_AccountSystem.BeginAccountSystemModification().ConfigureAwait(false);
        ///
        ///         if (!modifier.SignedInAccounts.Contains(account))
        ///         {
        ///             modifier.Add(account);
        ///             if (await PlatformSDK.GetPrimaryAccount() == account)
        ///                 modifier.MakePrimary(account);
        ///         }
        ///     }
        ///
        ///     private async void OnSignOut(PlatformAccount account)
        ///     {
        ///         await using var modifier = await m_AccountSystem.BeginAccountSystemModification().ConfigureAwait(false);
        ///         if (modifier.SignedInAccounts.Contains(account))
        ///             modifier.Remove(account);
        ///     }
        /// }
        /// </code>
        public GenericAccountSystem(IReadOnlyList<TAccount> initialAccounts = null,
            IPrimaryAccountSystemProvider primaryAccountSystemProvider = null,
            int primaryAccountIndex = -1,
            IAccountPickerSystemProvider accountPickerSystemProvider = null,
            IInputOwnershipSystem inputOwnershipSystem = null)
        {
            initialAccounts ??= Array.Empty<TAccount>();
            foreach (var account in initialAccounts)
            {
                Assert.IsNotNull(account, "One of the initial accounts is null");
                m_Accounts.Add(account);
            }

            if (primaryAccountSystemProvider != null)
            {
                Assert.IsTrue(primaryAccountIndex < initialAccounts.Count, "Primary account index is out of range");
                if (primaryAccountIndex >= 0)
                    m_PrimaryAccountSystem = new GenericPrimaryAccountSystem(primaryAccountSystemProvider, initialAccounts[primaryAccountIndex]);
                else
                    m_PrimaryAccountSystem = new GenericPrimaryAccountSystem(primaryAccountSystemProvider, null);
            }

            if (accountPickerSystemProvider != null)
            {
                m_AccountPickerSystem = new GenericAccountPickerSystem(accountPickerSystemProvider);
            }

            m_InputOwnershipSystem = inputOwnershipSystem;

            BuildSignedInAccountCollection();
        }

        private void BuildSignedInAccountCollection()
        {
            m_SignedInAccounts = new ReadOnlyCollection<IAccount>(m_Accounts.Select(a => (IAccount)a).ToList());
        }

        /// <summary>Begin account system modification.</summary>
        /// <remarks>
        /// <para>Call this method before starting any operations that might change the set of signed in accounts or the primary account, to ensure that accounts don't change in the meantime.</para>
        /// <para><see cref="GenericAccountSystem{TAccount}"/> allows only a single system modification at a time. This method will complete only after existing <see cref="IAccountModifier{TAccount}"/> instances are disposed.</para>
        /// </remarks>
        /// <returns>Returns when it is the turn of the caller to modify accounts.</returns>
        public Task<IAccountModifier<TAccount>> BeginAccountSystemModification()
        {
            lock (m_PendingAccountModifications)
            {
                if (m_CurrentAccountModifier == null && m_PendingAccountModifications.Count == 0)
                {
                    m_CurrentAccountModifier = new AccountSystemModifier(this, m_PrimaryAccountSystem?.CurrentPrimaryAccount, m_Accounts.ToArray());
                    return Task.FromResult<IAccountModifier<TAccount>>(m_CurrentAccountModifier);
                }
                else
                {
                    var completionSource = new TaskCompletionSource<IAccountModifier<TAccount>>();
                    m_PendingAccountModifications.Enqueue(completionSource);
                    return completionSource.Task;
                }
            }
        }

        private void EndAccountSystemModification(AccountSystemModifier accountSystemModifier)
        {
            lock (m_PendingAccountModifications)
            {
                if (accountSystemModifier != m_CurrentAccountModifier)
                    return;

                m_CurrentAccountModifier = null;
                if (m_PendingAccountModifications.Count > 0)
                {
                    m_CurrentAccountModifier = new AccountSystemModifier(this, m_PrimaryAccountSystem?.CurrentPrimaryAccount, m_Accounts.ToArray());
                    var completionSource = m_PendingAccountModifications.Dequeue();
                    completionSource.SetResult(m_CurrentAccountModifier);
                }
            }
        }

        private void Add(TAccount account)
        {
            m_Accounts.Add(account);
            BuildSignedInAccountCollection();
            SafeInvoker.Invoke(OnChange, account, AccountState.SignedIn);
        }

        private void Remove(TAccount account)
        {
            m_Accounts.Remove(account);
            BuildSignedInAccountCollection();
            SafeInvoker.Invoke(OnChange, account, AccountState.SignedOut);
        }

        private void MakePrimary(TAccount account)
        {
            if (m_PrimaryAccountSystem != null)
                m_PrimaryAccountSystem.CurrentPrimaryAccount = account;
        }

        private class GenericPrimaryAccountSystem : IPrimaryAccountSystem
        {
            public IAccount Current => m_CurrentPrimaryAccount;

            public TAccount CurrentPrimaryAccount
            {
                get => m_CurrentPrimaryAccount;
                set
                {
                    if (m_CurrentPrimaryAccount != value)
                    {
                        m_CurrentPrimaryAccount = value;
                        SafeInvoker.Invoke(OnChange);
                    }
                }
            }

            private TAccount m_CurrentPrimaryAccount;

            public event Action OnChange;

            private readonly IPrimaryAccountSystemProvider m_Provider;

            public GenericPrimaryAccountSystem(IPrimaryAccountSystemProvider provider, TAccount primaryPrimaryAccount)
            {
                m_Provider = provider;
                m_CurrentPrimaryAccount = primaryPrimaryAccount;
            }

            public Task<IAccount> Establish()
            {
                return m_Provider.Establish();
            }
        }

        private class GenericAccountPickerSystem : IAccountPickerSystem
        {
            private readonly IAccountPickerSystemProvider m_Provider;

            public GenericAccountPickerSystem(IAccountPickerSystemProvider provider)
            {
                m_Provider = provider;
            }

            public Task<IAccount> Show()
            {
                return m_Provider.Show();
            }
        }
    }
}
