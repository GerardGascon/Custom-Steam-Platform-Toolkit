using UnityEngine.Assertions;

namespace Unity.PlatformToolkit
{
    /// <summary>
    /// Build <see cref="ICapabilities"/>, <see cref="IAccountSystemCapabilities"/> and <see cref="IAccountCapabilities"/>,
    /// without the need to extend these interfaces. To use set properties on this object and then call <see cref="ToCapabilities"/>,
    /// to get back an <see cref="ICapabilities"/> object.
    /// </summary>
    internal class CapabilityBuilder
    {
        public bool LocalSavingSystem { get; set; }
        public bool AccountSupport { get; set; }
        public bool PrimaryAccount { get; set; }
        public bool PrimaryAccountEstablishLimited { get; set; }
        public bool AdditionalAccountSystem { get; set; }
        public bool AccountInputPairingSystem { get; set; }
        public bool AccountName { get; set; }
        public bool AccountPicture { get; set; }
        public bool AccountSavingSystem { get; set; }
        public bool AccountAchievementSystem { get; set; }
        public bool AccountManualSignOut { get; set; }

        public ICapabilities ToCapabilities()
        {
            return new GenericCapabilities
            (
                accounts: AccountSupport,
                primaryAccount: PrimaryAccount,
                accountPicker: AdditionalAccountSystem,
                inputOwnership: AccountInputPairingSystem,
                establishLimited: PrimaryAccountEstablishLimited,
                accountSaving: AccountSavingSystem,
                accountAchievements: AccountAchievementSystem,
                accountManualSignOut: AccountManualSignOut,
                localSaving: LocalSavingSystem
            );
        }

        private class GenericCapabilities : ICapabilities
        {
            public GenericCapabilities
            (
                bool accounts,
                bool primaryAccount,
                bool accountPicker,
                bool inputOwnership,
                bool establishLimited,
                bool accountSaving,
                bool accountAchievements,
                bool accountManualSignOut,
                bool localSaving
            )
            {
                if (primaryAccount)
                    Assert.IsTrue(accounts);
                if (accountPicker)
                    Assert.IsTrue(accounts);
                if (accountSaving)
                    Assert.IsTrue(accounts);
                if (accountAchievements)
                    Assert.IsTrue(accounts);
                if (accountManualSignOut)
                    Assert.IsTrue(accounts);
                if (inputOwnership)
                    Assert.IsTrue(accounts);
                if (establishLimited)
                    Assert.IsTrue(primaryAccount);

                Accounts = accounts;
                PrimaryAccount = primaryAccount;
                AccountPicker = accountPicker;
                InputOwnership = inputOwnership;
                PrimaryAccountEstablishLimited = establishLimited;
                AccountSaving = accountSaving;
                AccountAchievements = accountAchievements;
                AccountManualSignOut = accountManualSignOut;
                LocalSaving = localSaving;
            }

            public bool Accounts { get; }
            public bool PrimaryAccount { get; }
            public bool AccountPicker { get; }
            public bool InputOwnership { get; }
            public bool PrimaryAccountEstablishLimited { get; }
            public bool AccountSaving { get; }
            public bool AccountAchievements { get; }
            public bool AccountManualSignOut { get; }
            public bool LocalSaving { get; }
        }
    }
}
