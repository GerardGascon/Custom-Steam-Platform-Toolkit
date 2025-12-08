using System;

namespace Unity.PlatformToolkit.PlayMode
{
    internal interface INotificationManager
    {
        // <summary>
        // A notification will be displayed in the editor indicating that the given achievement has been unlocked
        // </summary>
        // <param name="achievementData"> The <see cref="PlayModeAchievementData"/> that will be displayed in the message</param>
        void AchievementUnlockedNotification(PlayModeAchievementData achievementData);

        /// <summary>
        /// A notification will be displayed in the editor indicating that a user must be selected from the Play Mode Controls.
        /// The notification will continue to be displayed until StopPendingEstablishUserNotification is called.
        /// </summary>
        void StartPendingEstablishUserNotification();

        /// <summary>
        /// Stop display of the notification started with StartPendingEstablishUserNotification.
        /// </summary>
        void StopPendingEstablishUserNotification();
    }
}
