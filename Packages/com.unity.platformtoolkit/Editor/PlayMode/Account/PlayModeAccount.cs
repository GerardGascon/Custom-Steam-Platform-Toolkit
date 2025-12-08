using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeAccount : IAccount, IDoubleSignOut
    {
        private readonly IPlayModeUserManager m_UserManager;
        private readonly IEnvironment m_Environment;
        private IAchievementSystem m_AchievementSystem;
        private ISavingSystem m_SavingSystem;

        public PlayModeAccountData AccountData { get; private set; }

        public AccountState State { get; set; }

        public PlayModeAccount(IEnvironment environment, IPlayModeUserManager userManager, PlayModeAccountData associatedData, ICapabilities capabilities)
        {
            State = AccountState.SignedIn;
            AccountData = associatedData;
            m_UserManager = userManager;
            m_Environment = environment;
            m_SavingSystem = new PlayModeSavingSystem(m_Environment, associatedData.Saves);
            m_AchievementSystem = capabilities.AccountAchievements ? new PlayModeAchievementSystem(m_Environment, associatedData.Achievements) : null;
        }

        public async Task<bool> SignOut()
        {
            await m_Environment.WaitIfPaused();

            bool ret = m_UserManager.CanSignOutAccount(AccountData) == SignOutStatus.Allowed;
            if (ret)
                m_UserManager.SignOutAccount(AccountData);

            return ret;
        }

        public bool HasAttribute<T>(string attributeName)
        {
            return false;
        }

        public async Task<string> GetName()
        {
            try
            {
                await m_Environment.WaitIfPaused();
                return AccountData?.PublicName ?? string.Empty;
            }
            catch (Exception e)
            {
                return AccountErrorHandling.HandleGetNameException(e);
            }
        }

        public async Task<Texture2D> GetPicture()
        {
            try
            {
                await m_Environment.WaitIfPaused();
                if (m_Environment.OfflineNetwork)
                {
                    return null;
                }
                return AccountData?.Picture;
            }
            catch (Exception e)
            {
                return AccountErrorHandling.HandleGetPictureException(e);
            }
        }

        public async Task<ISavingSystem> GetSavingSystem()
        {
            await m_Environment.WaitIfPaused();
            return m_SavingSystem;
        }

        public async Task<IAchievementSystem> GetAchievementSystem()
        {
            await m_Environment.WaitIfPaused();
            if (m_AchievementSystem == null)
            {
                throw new InvalidOperationException("Achievement System is not supported on this platform");
            }
            return m_AchievementSystem;
        }

        public bool TrySignOut()
        {
            if (State == AccountState.SignedOut)
                return false;
            State = AccountState.SignedOut;
            return true;
        }

        public Task CleanUpAfterSignOut()
        {
            m_SavingSystem = null;
            m_AchievementSystem = null;
            return Task.CompletedTask;
        }
    }
}
