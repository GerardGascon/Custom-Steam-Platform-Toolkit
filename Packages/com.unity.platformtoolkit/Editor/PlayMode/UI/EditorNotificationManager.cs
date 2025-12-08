using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class EditorNotificationManager : INotificationManager
    {
        private Task m_EstablishUserNotificationTask;
        private CancellationTokenSource m_NotificationCancelTokenSource;

        private const float k_UserEstablishNoficationFadeoutSeconds = 2.0f;
        private const int k_UserEstablishNoficationIntervalMs = (int)(k_UserEstablishNoficationFadeoutSeconds + 4.0f) * 1000;


        public void AchievementUnlockedNotification(PlayModeAchievementData achievementData)
        {
            var gameView = GetGameView();
            if (gameView == null)
            {
                Debug.LogWarning("Achievement Notification Failed - Unable to get the game view");
                return;
            }

            gameView.ShowNotification(new GUIContent($"{achievementData.Name} - Achievement Unlocked!"));
        }

        public void StartPendingEstablishUserNotification()
        {
            if (m_EstablishUserNotificationTask == null)
            {
                // Run a task to show repeated notifications until the user confirms an action
                m_NotificationCancelTokenSource = new CancellationTokenSource();
                _ = ShowPendingUserNotifications(m_NotificationCancelTokenSource.Token);
            }
        }

        public void StopPendingEstablishUserNotification()
        {
            m_NotificationCancelTokenSource?.Cancel();
            m_NotificationCancelTokenSource = null;
            m_EstablishUserNotificationTask = null;
        }

        private async Task ShowPendingUserNotifications(CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();

            var gameView = GetGameView();
            if (gameView == null)
            {
                Debug.LogWarning("Establish User Notification Failed - Unable to get the game view");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                gameView.ShowNotification(new GUIContent($"Awaiting user selection.{Environment.NewLine}{Environment.NewLine}See Play Mode Controls to select a user!"), k_UserEstablishNoficationFadeoutSeconds);
                await Task.Delay(k_UserEstablishNoficationIntervalMs, cancellationToken);
            }
        }

        private EditorWindow GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            return EditorWindow.GetWindow(type);
        }
    }
}
