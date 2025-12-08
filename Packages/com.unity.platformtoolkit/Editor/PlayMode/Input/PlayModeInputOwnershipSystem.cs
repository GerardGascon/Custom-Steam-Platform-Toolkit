#if INPUT_SYSTEM_AVAILABLE
using System;
using UnityEngine.InputSystem;

namespace Unity.PlatformToolkit.PlayMode
{
    internal sealed class PlayModeInputOwnershipSystem : AbstractInputOwnershipSystem, IDisposable
    {
        private class PlayModeInputDevice : IInputDevice
        {
            public string IdType { get; set; }

            public string Id { get; set; }
        }

        private PlayModeInputSystem m_InputSystem;
        private PlayModeAccountSystemManager m_AccountSystemManager;

        public PlayModeInputOwnershipSystem(PlayModeInputSystem inputSystem, PlayModeAccountSystemManager accountSystemManager)
        {
            m_InputSystem = inputSystem;
            m_AccountSystemManager = accountSystemManager;
            m_InputSystem.PairingChangedEvent += MarkPairingChanged;
            RegisterInputDeviceConverter<InputDevice>((inputDevice) =>
            {
                var playModeInputDevice = new PlayModeInputDevice();
                playModeInputDevice.Id = inputDevice.deviceId.ToString();
                playModeInputDevice.IdType = "deviceId";
                return playModeInputDevice;
            });
        }

        public override IAccount GetOwner(IInputDevice inputDevice)
        {
            var accountData = m_InputSystem.GetAccountByDeviceId(int.Parse(inputDevice.Id));
            if (accountData == null)
                return null;
            return m_AccountSystemManager.GetAccountFromData(accountData);
        }

        public void Dispose()
        {
            m_InputSystem.PairingChangedEvent -= MarkPairingChanged;
        }
    }
}
#endif // INPUT_SYSTEM_AVAILABLE
