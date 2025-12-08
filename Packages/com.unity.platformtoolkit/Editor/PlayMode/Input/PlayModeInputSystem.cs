#if INPUT_SYSTEM_AVAILABLE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Serialized data for the Play Mode Input System.
    /// </summary>
    [Serializable]
    internal class PlayModeInputSystemData
    {
        // Per documentation on SerializableDictionary, we create a subclass in order to serialize its contents.
        //TODO: Find a way to serialize the reference rather than the entire play mode account data object so we do not serialize twice or more
        [Serializable]
        internal class AccountByControllerId : SerializableDictionary<int, PlayModeAccountData>
        {
        }

        [SerializeField]
        // The keys in this dictionary == the IDs of currently connected gamepads.
        internal AccountByControllerId m_AccountByControllerId = new();
    }

    /// <summary>
    /// This class is for handling input and account pairings in the play mode controls system.
    /// </summary>
    internal class PlayModeInputSystem : IDisposable
    {
        private PlayModeInputSystemData m_SerializedData;

        public event Action PairingChangedEvent;

        // Used to persist writes in order to ensure changes are written to disk.
        // Set by the ScriptableObject that owns this object.
        public ScriptableObjectDataChangePersistor Persistor { private get; set; }
        public PlayModeUserManager UserManager { private get; set; }

        public PlayModeInputSystem(PlayModeInputSystemData serializedData, ScriptableObjectDataChangePersistor persistor, PlayModeUserManager userManager)
        {
            m_SerializedData = serializedData ?? throw new ArgumentNullException(nameof(serializedData));
            Persistor = persistor ?? throw new ArgumentNullException(nameof(persistor));
            UserManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            var accountByControllerId = m_SerializedData.m_AccountByControllerId;

            var connectedGamepadIds = Gamepad.all.Select(gamepad => gamepad.deviceId);
            var pairedGamepadIds = m_SerializedData.m_AccountByControllerId.Keys.ToArray();

            //Ensuring m_AccountByControllerId contains all connected gamepads
            foreach (var gamepadId in connectedGamepadIds)
            {
                if (!pairedGamepadIds.Contains(gamepadId))
                    m_SerializedData.m_AccountByControllerId.Add(gamepadId, null);
            }

            foreach (var gamepadId in pairedGamepadIds)
            {
                if (!connectedGamepadIds.Contains(gamepadId))
                    m_SerializedData.m_AccountByControllerId.Remove(gamepadId);
            }

            var accounts = UserManager.AccountData;
            foreach (var device in accountByControllerId.Keys.ToArray())
            {
                var account = accountByControllerId[device];
                if (!accounts.Contains(account))
                    m_SerializedData.m_AccountByControllerId[device] = null;
            }

            UserManager.AccountStateChangeEvent += OnAccountChange;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        public void Dispose()
        {
            UserManager.AccountStateChangeEvent -= OnAccountChange;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        /// <summary>
        /// Assigns the given deviceId to the given play mode account data.
        /// If this is a new device assignment, this triggers a PairingChangedEvent.
        /// This will overwrite any previous user assignment for this device, since devices can only be assigned to a single account
        /// </summary>
        /// <param name="deviceId">The id of the device that will be paired to the given account.</param>
        /// <param name="accountData">The account data that will be paired to the device. This account has to be signed-in.</param>
        /// <exception cref="ArgumentException">This is thrown if the given deviceId does not exist</exception>
        /// <exception cref="ArgumentNullException">This is thrown if the given account data is null</exception>
        /// <exception cref="InvalidOperationException">This is thrown when the given account is not signed-in</exception>
        public void Assign(int deviceId, PlayModeAccountData accountData)
        {
            var accountByControllerId = m_SerializedData.m_AccountByControllerId;

            if (!accountByControllerId.ContainsKey(deviceId))
                throw new ArgumentException("The given device id does not exist");

            if (accountData == null)
                throw new ArgumentNullException("The given account data is null");

            if (!UserManager.IsAccountSignedIn(accountData))
                throw new InvalidOperationException("Cannot assign device to an account that is not signed-in");

            if (accountByControllerId[deviceId] == accountData)
                return;

            accountByControllerId[deviceId] = accountData;
            Persistor.PersistWrites();
            PairingChangedEvent?.Invoke();
        }

        /// <summary>
        /// Unassigns the given deviceId from the assigned play mode account data.
        /// The input pair will still exist but the deviceId will be paired to null.
        /// The PairingChangedEvent will be triggered upon successful unpairing.
        /// </summary>
        /// <param name="deviceId">The id of the device that will be unpaired.</param>
        /// <exception cref="ArgumentException">This is thrown if the given deviceId does not exist</exception>
        /// <exception cref="InvalidOperationException">This is thrown when the given account is not signed-in</exception>
        public void Unassign(int deviceId)
        {
            var accountByControllerId = m_SerializedData.m_AccountByControllerId;

            if (!accountByControllerId.ContainsKey(deviceId))
                throw new ArgumentException("The given device id does not exist");

            if (accountByControllerId[deviceId] == null)
                return;

            accountByControllerId[deviceId] = null;
            Persistor.PersistWrites();
            PairingChangedEvent?.Invoke();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            //We only support Gamepads
            if (device as Gamepad is null)
                return;

            var accountByControllerId = m_SerializedData.m_AccountByControllerId;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Enabled:
                case InputDeviceChange.Reconnected:
                    if (accountByControllerId.ContainsKey(device.deviceId))
                        return;

                    //Ensuring m_AccountByControllerId contains all connected gamepads
                    accountByControllerId.Add(device.deviceId, null);
                    Persistor.PersistWrites();
                    PairingChangedEvent?.Invoke();
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    if (!accountByControllerId.ContainsKey(device.deviceId))
                        return;

                    accountByControllerId.Remove(device.deviceId);
                    Persistor.PersistWrites();
                    PairingChangedEvent?.Invoke();
                    break;
            }
        }

        private void OnAccountChange(PlayModeAccountData account, AccountState state)
        {
            if (state == AccountState.SignedOut)
            {
                var accountByControllerId = m_SerializedData.m_AccountByControllerId;

                foreach(var device in accountByControllerId.Keys.ToArray())
                {
                    var pairedAccount = accountByControllerId[device];
                    //We don't return after this because one account can be assigned to multiple devices
                    if (pairedAccount == account)
                        Unassign(device);
                }
            }
        }

        /// <summary>
        /// Get the dictionary of devices account pairs.
        /// This dictionary includes devices that are unassigned (not assigned to any user)
        /// </summary>
        /// <returns>
        /// An <see cref="IReadOnlyDictionary{InputDevice, PlayModeAccountData}"/> including all device pairs.
        /// Where the key is the input device, and the value is the account data (null if no account is assigned).
        /// </returns>
        public IReadOnlyDictionary<InputDevice, PlayModeAccountData> GetAccountDevicePairs()
        {
            var accountByInputDevice = new Dictionary<InputDevice, PlayModeAccountData>();
            foreach (var(deviceId, account) in m_SerializedData.m_AccountByControllerId)
            {
                var device = Gamepad.all.FirstOrDefault(g => g.deviceId == deviceId);
                if (device == null)
                {
                    //This should never be the case
                    throw new KeyNotFoundException($"Unable to find gamepad with id '{deviceId}'");
                }
                accountByInputDevice[device] = account;
            }
            return accountByInputDevice;
        }

        /// <summary>
        /// Get the account paired to the given device id.
        /// </summary>
        /// <param name="deviceId">The id of the device</param>
        /// <returns>
        /// An <see cref="PlayModeAccountData"/> representing the account that is currently paired to the device.
        /// null will be returned if device is not paired to any account
        /// or if the device is not a gamepad.
        /// </returns>
        public PlayModeAccountData GetAccountByDeviceId(int deviceId)
        {
            //m_AccountByControllerId only contains gamepads
            var found = m_SerializedData.m_AccountByControllerId.TryGetValue(deviceId, out var account);
            return found ? account : null;
        }
    }
}
#endif // INPUT_SYSTEM_AVAILABLE
