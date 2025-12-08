using System;
using System.Threading.Tasks;

namespace Unity.PlatformToolkit.PlayMode
{
    internal interface IEnvironment
    {
        /// <summary>
        /// Notification manager used to send system notifications.
        /// </summary>
        INotificationManager NotificationManager { get; }

        /// <summary>
        /// Simulate Offline Network
        /// </summary>
        bool OfflineNetwork { get; set; }

        /// <summary>
        /// Simulate Full Storage
        /// </summary>
        bool FullStorage { get; set; }

        /// <summary>
        /// Function delay and pausing manager for the environment configuration
        /// </summary>
        ITaskDelayer Delayer { get; }

        /// <summary>
        /// Calls pausing time, used to pause callers of <see cref="WaitIfPaused"/> for the given amount of time
        /// </summary>
        TimeSpan CallsPausingTime { get; set; }

        /// <summary>
        /// Wait if paused
        /// </summary>
        /// <returns> A <see cref="Task"/> that can be awaited upon </returns>
        Task WaitIfPaused();
    }
}
