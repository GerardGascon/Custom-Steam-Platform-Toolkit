using System;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Used to set up a play mode state from serialized settings for edit-time and play-time.
    /// This emulates a platform's runtime/OS, and is not to be confused with a Platform Toolkit runtime.
    /// </summary>
    internal class PlayModeControlsRuntime : IDisposable
    {
        private IPlayModeCapability m_Capability;
        public IPlayModeCapability Capability
        {
            get
            {
                m_LifetimeToken.ThrowOnDisposedAccess();
                return m_Capability;
            }
            private set
            {
                m_LifetimeToken.ThrowOnDisposedAccess();
                m_Capability = value;
            }
        }

        private PlayModeEnvironment m_Environment;
        public IEnvironment Environment
        {
            get
            {
                m_LifetimeToken.ThrowOnDisposedAccess();
                return m_Environment;
            }
        }


        private PlayModeUserManager m_UserManager;
        public IPlayModeUserManager UserManager
        {
            get
            {
                m_LifetimeToken.ThrowOnDisposedAccess();
                return m_UserManager;
            }
        }

#if INPUT_SYSTEM_AVAILABLE

        private PlayModeInputSystem m_InputSystem;
        public PlayModeInputSystem PlayModeInputSystem
        {
            get
            {
                m_LifetimeToken.ThrowOnDisposedAccess();
                return m_InputSystem;
            }
        }
#endif

        private PlayModeAccessor m_PlayModeAccessor;

        GenericLifetimeToken m_LifetimeToken = new GenericLifetimeToken();

        public PlayModeControlsRuntime(PlayModeControlsSettings settings, IPlayModeCapability capability, ScriptableObjectDataChangePersistor persistor)
        {
            Capability = capability;
            m_PlayModeAccessor = settings.PlayModeAccessor;

            m_Environment = new PlayModeEnvironment(settings.m_Environment);
            m_UserManager = new PlayModeUserManager(settings.m_Accounts, persistor, m_Environment, capability,  m_PlayModeAccessor.IsPlaying);

#if INPUT_SYSTEM_AVAILABLE
            m_InputSystem = new PlayModeInputSystem(settings.m_InputSystem, persistor, m_UserManager);
#endif
        }

        public void Dispose()
        {
            m_LifetimeToken.TryAtomicDispose();

            m_Environment?.Dispose();
            m_Environment = null;

            m_UserManager?.Dispose();
            m_UserManager = null;

#if INPUT_SYSTEM_AVAILABLE
            m_InputSystem?.Dispose();
            m_InputSystem = null;
#endif
        }
    }
}
