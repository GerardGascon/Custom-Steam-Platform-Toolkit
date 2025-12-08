using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Threading;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Serialized data for the Play Mode Environment.
    /// </summary>
    [Serializable]
    internal class PlayModeEnvironmentData
    {
        [SerializeField]
        private int m_SimulateLongRunningTaskDelayInMilliseconds = 0;

        public TimeSpan CallsPausingTime
        {
            get => TimeSpan.FromMilliseconds(m_SimulateLongRunningTaskDelayInMilliseconds);
            set => m_SimulateLongRunningTaskDelayInMilliseconds = (int)value.TotalMilliseconds;
        }

        public bool OfflineNetwork { get; set; } = false;

        public bool FullStorage { get; set; } = false;
    }

    /// <summary>
    /// Class for managing the play mode environment in Edit and Play Mode.
    /// </summary>
    internal class PlayModeEnvironment : IEnvironment
    {
        private PlayModeEnvironmentData m_SerializedData;
        private CancellationTokenSource m_WaitCancellationSource = new CancellationTokenSource();

        public INotificationManager NotificationManager { get; set; } = new EditorNotificationManager();

        public bool OfflineNetwork
        {
            get { return m_SerializedData.OfflineNetwork; }
            set { m_SerializedData.OfflineNetwork = value; }
        }

        public bool FullStorage
        {
            get { return m_SerializedData.FullStorage; }
            set { m_SerializedData.FullStorage = value; }
        }

        public TimeSpan CallsPausingTime
        {
            get { return m_SerializedData.CallsPausingTime; }
            set { m_SerializedData.CallsPausingTime = value; }
        }

        public ITaskDelayer Delayer { get; set; }

        public PlayModeEnvironment(PlayModeEnvironmentData serializedData)
        {
            m_SerializedData = serializedData ?? throw new ArgumentNullException(nameof(serializedData));
            Delayer = new PlayModeDelayer(m_WaitCancellationSource);
        }


        public async Task WaitIfPaused()
        {
            await Delayer.Delay(CallsPausingTime);
        }

        public void Dispose()
        {
            m_WaitCancellationSource.Cancel();
        }
    }

    internal class PlayModeDelayer : ITaskDelayer
    {
        private CancellationTokenSource m_WaitCancellationSource;

        public PlayModeDelayer(CancellationTokenSource cancellationSource)
        {
            m_WaitCancellationSource = cancellationSource;
        }

        public async Task Delay(TimeSpan waitFor)
        {
            if (!m_WaitCancellationSource.IsCancellationRequested)
            {
                try
                {
                    if (waitFor.TotalMilliseconds > 0)
                        await Task.Delay(waitFor, m_WaitCancellationSource.Token);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}
