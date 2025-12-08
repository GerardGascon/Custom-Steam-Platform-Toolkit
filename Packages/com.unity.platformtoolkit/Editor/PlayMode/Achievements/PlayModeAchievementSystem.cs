using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.PlatformToolkit.Editor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeAchievementSystem : IAchievementSystem
    {
        private INotificationManager m_NotificationManager;
        private PlayModeAccountAchievementData m_AchievementData;

        public PlayModeAchievementSystem(IEnvironment environment, PlayModeAccountAchievementData achievementData)
        {
            m_NotificationManager = environment.NotificationManager ?? throw new ArgumentNullException(nameof(environment.NotificationManager));
            // Make sure the achievement data has updated names.
            achievementData.UpdateAchievementList();
            m_AchievementData = achievementData;
        }

        public void Unlock(string id)
        {
            var playModeAchievement = m_AchievementData.GetAchievement(id);
            if (playModeAchievement.Unlocked)
                return;
            switch (playModeAchievement.Category)
            {
                case UnlockType.Single:
                    playModeAchievement.Unlocked = true;
                    m_NotificationManager.AchievementUnlockedNotification(playModeAchievement);
                    break;
                case UnlockType.Progressive:
                    playModeAchievement.Progress = playModeAchievement.TargetValue;
                    m_NotificationManager.AchievementUnlockedNotification(playModeAchievement);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown category {playModeAchievement.Category}");
            }
        }

        public void UpdateProgress(string id, int progress)
        {
            var playModeAchievement = m_AchievementData.GetAchievement(id);
            if (playModeAchievement.Unlocked)
                return;
            switch (playModeAchievement.Category)
            {
                case UnlockType.Single:
                    if (progress > 0)
                    {
                        playModeAchievement.Unlocked = true;
                        m_NotificationManager.AchievementUnlockedNotification(playModeAchievement);
                    }
                    break;
                case UnlockType.Progressive:
                    if (progress >= playModeAchievement.Progress)
                    {
                        playModeAchievement.Progress = progress;
                        if (progress == playModeAchievement.TargetValue)
                            m_NotificationManager.AchievementUnlockedNotification(playModeAchievement);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown category {playModeAchievement.Category}");
            }
        }
    }
}
