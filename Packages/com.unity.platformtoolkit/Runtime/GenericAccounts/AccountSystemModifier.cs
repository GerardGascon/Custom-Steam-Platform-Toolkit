using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit
{
    internal partial class GenericAccountSystem<TAccount> : IAccountSystem where TAccount : class, IAccount, IDoubleSignOut
    {
        private class AccountSystemModifier : IAccountModifier<TAccount>
        {
            private bool m_Disposed;
            private GenericAccountSystem<TAccount> m_AccountSystem;

            private readonly List<(TAccount account, AccountState state)> m_AccountEvents = new();
            private readonly Queue<TAccount> m_AccountsToCleanUpAfterRemoval = new();

            private TAccount m_NewPrimaryAccount;

            public AccountSystemModifier(GenericAccountSystem<TAccount> accountSystem, TAccount currentPrimaryAccount, IReadOnlyList<TAccount> signedInAccounts)
            {
                m_AccountSystem = accountSystem;
                CurrentPrimaryAccount = currentPrimaryAccount;
                SignedInAccounts = signedInAccounts;

                m_NewPrimaryAccount = CurrentPrimaryAccount;
            }

            public TAccount CurrentPrimaryAccount { get; }
            public IReadOnlyList<TAccount> SignedInAccounts { get; }

            public void Add(TAccount account)
            {
                DisposeCheck();
                if (SignedInAccounts.Contains(account))
                {
                    throw new InvalidOperationException(
                        "Attempting to add an account that is already signed in.");
                }

                foreach (var accountEvent in m_AccountEvents)
                {
                    if (accountEvent.account == account)
                        return;
                }

                m_AccountEvents.Add((account, AccountState.SignedIn));
            }

            public void Remove(TAccount account)
            {
                DisposeCheck();
                if (!SignedInAccounts.Contains(account))
                {
                    throw new InvalidOperationException(
                        "Attempting to remove an account that was not signed in at the beginning of modification.");
                }

                foreach (var accountEvent in m_AccountEvents)
                {
                    if (accountEvent.account == account)
                        return;
                }

                m_AccountEvents.Add((account, AccountState.SignedOut));
                if (account.TrySignOut())
                    m_AccountsToCleanUpAfterRemoval.Enqueue(account);

                if (account == CurrentPrimaryAccount)
                    MakePrimary(null);
            }

            public void MakePrimary(TAccount account)
            {
                DisposeCheck();
                if (m_NewPrimaryAccount == account)
                {
                    return;
                }

                if (account == null)
                {
                    m_NewPrimaryAccount = null;
                    return;
                }

                var accountSignedInBeforeModification = false;
                foreach (var signedInAccount in SignedInAccounts)
                {
                    if (account == signedInAccount)
                    {
                        accountSignedInBeforeModification = true;
                        break;
                    }
                }

                if (accountSignedInBeforeModification)
                {
                    foreach (var accountEvent in m_AccountEvents)
                    {
                        if (accountEvent.account == account && accountEvent.state == AccountState.SignedOut)
                        {
                            throw new InvalidOperationException("Attempting to make primary a signed out account.");
                        }
                    }
                    m_NewPrimaryAccount = account;
                    return;
                }
                else
                {
                    foreach (var accountEvent in m_AccountEvents)
                    {
                        if (accountEvent.account == account && accountEvent.state == AccountState.SignedIn)
                        {
                            m_NewPrimaryAccount = account;
                            return;
                        }
                    }
                    throw new InvalidOperationException("Attempting to make primary an account that is not signed in.");
                }
            }

            private async Task InvokePendingEvents()
            {
                await Awaitable.MainThreadAsync();
                foreach (var accountEvent in m_AccountEvents)
                {
                    if (accountEvent.state == AccountState.SignedIn)
                        m_AccountSystem.Add(accountEvent.account);
                    else if (accountEvent.state == AccountState.SignedOut)
                        m_AccountSystem.Remove(accountEvent.account);
                }

                if (m_NewPrimaryAccount != CurrentPrimaryAccount)
                    m_AccountSystem.MakePrimary(m_NewPrimaryAccount);
            }

            private void DisposeCheck()
            {
                if (m_Disposed)
                    throw new ObjectDisposedException(nameof(AccountSystemModifier));
            }

            public async ValueTask DisposeAsync()
            {
                if (m_Disposed)
                    return;
                m_Disposed = true;

                await InvokePendingEvents().ConfigureAwait(false);
                m_AccountSystem.EndAccountSystemModification(this);

                while (m_AccountsToCleanUpAfterRemoval.TryDequeue(out var account))
                {
                    _ = account.CleanUpAfterSignOut();
                }
            }
        }
    }
}
