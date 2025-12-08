using System;
using System.Collections.Generic;
using Unity.PlatformToolkit.Editor;
using Unity.Properties;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// A UI-bindable object for play mode controls UI that has a lifetime tied to a settings asset. UI should bind to this.
    ///
    /// An instance of this object should be created per-asset since it's shared between both the Play Mode Controls UI and Inspector views.
    /// This allows for both UIs to hear events that relate to that data, without confusing events from unrelated assets.
    /// There's only expected to be a single viewed runtime setup per-settings asset as well so that object doesn't share the same issue.
    /// </summary>
    internal class PlayModeControlsViewModel : INotifyBindablePropertyChanged
    {
        private PlayModeControlsSettings m_Settings;
        private PlayModeControlsRuntime m_Runtime;

        // Cached objects for unregistering callbacks
        private ICapabilitySelector m_RegisteredCapabilitySelector;
        private IPlayModeUserManager m_RegisteredUserManager;

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        internal WeakEvent OnAccountControlsInvalidated = new();
        internal WeakEvent OnEnvironmentControlsInvalidated = new();
        internal WeakEvent OnCapabilitiesInvalidated = new();
        internal WeakEvent OnDisposed = new();

        /// <summary>
        /// Values have their own <see cref="IPlayModeCapability.Title"/>, but these titles can repeat.
        /// In order to present each <see cref="IPlayModeCapability"/> with a unique name a suffix is added to repeating Titles.
        /// Keys are <see cref="IPlayModeCapability.Title"/> of the Value, plus a suffix if there are other capabilities with the same Title.
        /// </summary>
        private readonly Dictionary<string, IPlayModeCapability> m_PlayModeCapabilityOptions = new();
        private readonly List<string> m_PlayModeCapabilityOptionNames = new();


        public bool IsValid => (m_Settings != null) && (m_Runtime != null) && m_Settings.HasBeenEnabled;
        public bool AreSettingsValid => (m_Settings != null) && m_Settings.HasBeenEnabled;
        public bool IsPlaying => m_Settings.PlayModeAccessor.IsPlaying;

        internal PlayModeControlsSettings BoundSettings => m_Settings;
        internal PlayModeControlsRuntime BoundRuntime => m_Runtime;

        [CreateProperty] public bool ShowInputDeviceMappingHelpBox => !Capability.SupportsAccountInputOwnership;

        [CreateProperty] public bool ShowInputDeviceMappingLabel
        {
            get
            {
#if INPUT_SYSTEM_AVAILABLE
            return m_Runtime.PlayModeInputSystem.GetAccountDevicePairs().Count == 0 && Capability.SupportsAccountInputOwnership;
#else
            return false;
#endif
            }
        }

        public bool SupportAccountOwnership => Capability.SupportsAccountInputOwnership;

        private void CheckIsValid()
        {
            Assert.IsTrue(IsValid, "IsValid should be used to check if it's valid to call this.");
        }

        private void CheckAreSettingsValid()
        {
            Assert.IsTrue(AreSettingsValid, "AreSettingsValid should be used to check if it's valid to call this.");
        }


        /// <summary>
        /// Events for property changes, to notify the UI and listeners of modified data.
        /// </summary>
        #region PropertyChangeEvents

        private void CapabilityOptionsChanged()
        {
            CacheCapabilityOptions();

            OnAccountControlsInvalidated?.Invoke();
            OnCapabilitiesInvalidated?.Invoke();
        }

        private void AccountDataChanged()
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(AccountData)));
            OnAccountControlsInvalidated?.Invoke();
        }

        private void AccountStateChanged(PlayModeAccountData accountData, AccountState state)
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(SignedInAccounts)));
            OnAccountControlsInvalidated?.Invoke();
        }

        private void PrimaryAccountChanged()
        {
            OnAccountControlsInvalidated?.Invoke();
        }

        private void PickAccountRequestChanged()
        {
            OnAccountControlsInvalidated?.Invoke();
        }

        #endregion

        /// <summary>
        /// Account management methods and bindable properties.
        /// </summary>
        #region Accounts

        internal void CreateNewAccount()
        {
            CheckIsValid();
            m_Runtime.UserManager.CreateNewAccount();
        }

        internal void RemoveAccount(int index)
        {
            CheckIsValid();
            var accountData = AccountData[index];
            m_Runtime.UserManager.DeleteAccount(accountData);
        }

        internal PlayModeAccountData PrimaryAccountData
        {
            get
            {
                CheckIsValid();
                return m_Runtime.UserManager.PrimaryAccountData;
            }
        }

        [CreateProperty]
        internal IReadOnlyList<PlayModeAccountData> AccountData
        {
            get
            {
                CheckAreSettingsValid();
                return m_Settings.m_Accounts.AccountData;
            }
        }

        internal IReadOnlyList<PlayModeAccountData> SignedInAccounts
        {
            get
            {
                CheckAreSettingsValid();
                return m_Settings.m_Accounts.SignedInAccounts;
            }
        }

        internal SetPrimaryAccountStatus CanSetAccountToPrimaryManually(PlayModeAccountData accountData)
        {
            CheckIsValid();
            return m_Runtime.UserManager.CanSetAccountToPrimaryManually(accountData);
        }

        internal SignInStatus CanSignInAccountManually(PlayModeAccountData accountData)
        {
            CheckIsValid();
            return m_Runtime.UserManager.CanSignInAccountManually(accountData);
        }

        internal SignInStatus CanSignInAccount(PlayModeAccountData accountData)
        {
            CheckIsValid();
            return m_Runtime.UserManager.CanSignInAccount(accountData);
        }

        internal SignOutStatus CanSignOutAccount(PlayModeAccountData accountData)
        {
            CheckIsValid();
            return m_Runtime.UserManager.CanSignOutAccount(accountData);
        }

        internal bool IsAccountSignedIn(PlayModeAccountData accountData)
        {
            CheckIsValid();
            return m_Runtime.UserManager.IsAccountSignedIn(accountData);
        }

        internal void SignInAccount(PlayModeAccountData accountData)
        {
            CheckIsValid();
            m_Runtime.UserManager.SignInAccount(accountData);
        }

        internal void SignOutAccount(PlayModeAccountData accountData)
        {
            CheckIsValid();
            m_Runtime.UserManager.SignOutAccount(accountData);
        }

        internal void SetToPrimary(PlayModeAccountData accountData)
        {
            CheckIsValid();
            m_Runtime.UserManager.SetToPrimary(accountData);
        }

        internal void OnAccountPicked(PlayModeAccountData accountData)
        {
            CheckIsValid();
            m_Runtime.UserManager.OnAccountPicked(accountData);
        }

        internal void RefusedAccountSelection()
        {
            CheckIsValid();
            m_Runtime.UserManager.OnAccountPickRefused();
        }

        internal bool IsPickAccountRequestActive
        {
            get
            {
                CheckIsValid();
                return m_Runtime.UserManager.IsPickAccountRequestActive;
            }
        }
        #endregion

        /// <summary>
        /// Input assignment management and bindable properties.
        /// </summary>
        #region Input Assignment
#if INPUT_SYSTEM_AVAILABLE
        internal IReadOnlyDictionary<UnityEngine.InputSystem.InputDevice, PlayModeAccountData> GetAccountDevicePairs()
        {
            CheckIsValid();
            return m_Runtime.PlayModeInputSystem.GetAccountDevicePairs();
        }

        internal void UnassignInputDevice(int deviceId)
        {
            CheckIsValid();
            m_Runtime.PlayModeInputSystem.Unassign(deviceId);
        }

        internal void AssignInputDevice(int deviceId, PlayModeAccountData accountData)
        {
            CheckIsValid();
            m_Runtime.PlayModeInputSystem.Assign(deviceId, accountData);
        }
#endif
        #endregion

        /// <summary>
        /// Environment settings and bindable properties.
        /// </summary>
        #region Environment Settings

        internal Dictionary<string, IPlayModeCapability> CapabilityOptions => m_PlayModeCapabilityOptions;
        internal List<string> CapabilityOptionNames => m_PlayModeCapabilityOptionNames;


        [CreateProperty]
        internal IPlayModeCapability Capability
        {
            get
            {
                CheckAreSettingsValid();
                return m_Settings.CapabilitySelector.CurrentCapability;
            }
            set
            {
                CheckAreSettingsValid();
                m_Settings.CapabilitySelector.CurrentCapability = value;
            }
        }

        [CreateProperty]
        internal bool OfflineNetwork
        {
            get
            {
                CheckIsValid();
                return m_Runtime.Environment.OfflineNetwork;
            }

            set
            {
                CheckIsValid();
                m_Runtime.Environment.OfflineNetwork = value;
            }
        }

        [CreateProperty]
        internal bool FullStorage
        {
            get
            {
                CheckIsValid();
                return m_Runtime.Environment.FullStorage;
            }

            set
            {
                CheckIsValid();
                m_Runtime.Environment.FullStorage = value;
            }
        }

        [CreateProperty]
        internal TimeSpan CallsPausingTime
        {
            get
            {
                CheckIsValid();
                return m_Runtime.Environment.CallsPausingTime;
            }

            set
            {
                CheckIsValid();
                m_Runtime.Environment.CallsPausingTime = value;
            }
        }

        #endregion

        /// <summary>
        /// Local Saving system data access.
        /// </summary>
        #region Local Saving Data

        internal bool SupportsLocalSaving
        {
            get
            {
                CheckIsValid();
                return m_Runtime.Capability.SupportsLocalSaving;
            }
        }

        internal bool SupportsAccounts
        {
            get
            {
                CheckIsValid();
                return !(m_Runtime.Capability.PrimaryAccountBehaviour == PrimaryAccountBehaviour.NotSupported &&
                    m_Runtime.Capability.AdditionalAccountBehaviour == AdditionalAccountBehaviour.NotSupported);
            }
        }

        [CreateProperty]
        internal PlayModeSaveData LocalSaveData
        {
            get
            {
                CheckAreSettingsValid();
                return m_Settings.LocalSaveData;
            }
        }

        #endregion

        internal PlayModeControlsViewModel(PlayModeControlsSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("PlayModeControlsViewModel cannot be constructed with a null settings asset.");

            if (!settings.HasBeenEnabled)
                throw new ArgumentException("Settings should only be bound for enabled PlayModeControlsSettings assets.");

            m_Settings = settings;
            if (m_Settings.Runtime != null)
            {
                BindRuntime(m_Settings.Runtime, m_Settings, shouldNotify: false);
            }

            m_RegisteredCapabilitySelector = m_Settings.CapabilitySelector;
            m_RegisteredCapabilitySelector.CapabilitiesCreatedOrDeleted += CapabilityOptionsChanged;
            CacheCapabilityOptions();
        }

        internal void Dispose()
        {
            ClearCachedCapabilityOptions();

            if (m_RegisteredCapabilitySelector != null)
            {
                m_RegisteredCapabilitySelector.CapabilitiesCreatedOrDeleted -= CapabilityOptionsChanged;
            }

            bool settingsUnbound = m_Settings != null;
            m_Settings = null;
            m_RegisteredCapabilitySelector = null;

            UnbindRuntime();

            if (settingsUnbound)
                OnDisposed?.Invoke();
        }

        internal void BindRuntime(PlayModeControlsRuntime runtime, PlayModeControlsSettings expectedSettings, bool shouldNotify = true)
        {
            if (runtime == null)
                throw new ArgumentNullException("A null runtime shouldn't be bound explicitly as one should always exist with valid settings. Call BindSettings with null instead to remove bindings.");

            if (m_Settings == null)
                throw new ArgumentException("No settings are bound for this view. A runtime cannot be bound without bound settings.");

            if (expectedSettings != m_Settings)
                throw new ArgumentException("The bound settings do not match the ones passed as an argument. This is likely a runtime for a different asset.");

            UnbindRuntime();

            Assert.IsNull(m_Runtime);
            Assert.IsNull(m_RegisteredUserManager);

            m_Runtime = runtime;
            if (m_Runtime != null)
            {
                m_RegisteredUserManager = m_Runtime.UserManager;
                m_RegisteredUserManager.AccountDataUpdateEvent += AccountDataChanged;
                m_RegisteredUserManager.AccountStateChangeEvent += AccountStateChanged;
                m_RegisteredUserManager.PrimaryAccountChangeEvent += PrimaryAccountChanged;
                m_RegisteredUserManager.PickAccountRequestReceivedEvent += PickAccountRequestChanged;
                m_RegisteredUserManager.PickAccountRequestResolvedEvent += PickAccountRequestChanged;
            }

            if (shouldNotify)
            {
                NotifyOfBindingUpdates();
            }
        }

        private void NotifyOfBindingUpdates()
        {
            // The runtime objects have changed so everything is invalid

            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(ShowInputDeviceMappingHelpBox)));
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(ShowInputDeviceMappingLabel)));
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(Capability)));
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(AccountData)));
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(SignedInAccounts)));

            OnAccountControlsInvalidated?.Invoke();
            OnCapabilitiesInvalidated?.Invoke();
            OnEnvironmentControlsInvalidated?.Invoke();
        }


        private void UnbindRuntime()
        {
            if (m_Runtime != null)
            {
                m_RegisteredUserManager.AccountDataUpdateEvent -= AccountDataChanged;
                m_RegisteredUserManager.AccountStateChangeEvent -= AccountStateChanged;
                m_RegisteredUserManager.PrimaryAccountChangeEvent -= PrimaryAccountChanged;
                m_RegisteredUserManager.PickAccountRequestReceivedEvent -= PickAccountRequestChanged;
                m_RegisteredUserManager.PickAccountRequestResolvedEvent -= PickAccountRequestChanged;
            }

            m_Runtime = null;
            m_RegisteredUserManager = null;
        }

        private void ClearCachedCapabilityOptions()
        {
            m_PlayModeCapabilityOptions.Clear();
            m_PlayModeCapabilityOptionNames.Clear();
        }

        private void CacheCapabilityOptions()
        {
            ClearCachedCapabilityOptions();

            var capabilityAssets = m_RegisteredCapabilitySelector.Capabilities;
            foreach (var titleAndValue in capabilityAssets.AssignUniqueString(c => c.Title))
            {
                m_PlayModeCapabilityOptions.Add(titleAndValue.UniqueString, titleAndValue.Value);
                m_PlayModeCapabilityOptionNames.Add(titleAndValue.UniqueString);
            }
        }
    }

    /// <summary>
    /// A bindable object for when no data is actually bound. This is used to meet requirements for any UI which still evaluates bindings whilst data is not present.
    /// </summary>
    internal class NullPlayModeControlsViewModel
    {
        [CreateProperty] public bool ShowInputDeviceMappingHelpBox => false;
        [CreateProperty] public bool ShowInputDeviceMappingLabel => false;
    }
}
