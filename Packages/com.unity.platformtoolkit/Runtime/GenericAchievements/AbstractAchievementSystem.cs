using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.PlatformToolkit
{
    internal abstract class AbstractAchievementSystem<T> : IAsyncDisposable, IAchievementSystem
        where T : AchievementDefinition
    {
        private const int k_DefaultTimeBetweenUpdatesMilliseconds = 100;

        private readonly ILifetimeToken m_ParentLifetimeToken;
        // Don't re-initialize a system until all previous initializations have finished
        private readonly SemaphoreSlim m_InitializationSemaphore = new SemaphoreSlim(0, 1);
        private readonly TaskStop m_InactivityStop = new(true);
        private readonly TaskStop m_DisabledStop = new(true);

        private readonly int m_TimeBetweenUpdatesMilliseconds;
        private readonly AsyncLock m_UpdateLock = new();

        private DateTime m_LastUpdateTime = DateTime.Now;

        private readonly ConcurrentQueue<AchievementUpdateCommand<T>> m_NewUpdateCommands = new();
        private readonly Dictionary<string, AchievementUpdateCommand<T>> m_PendingUpdateCommands = new();

        private readonly IReadOnlyList<T> m_AchievementDefinitions;
        protected IReadOnlyList<T> AchievementDefinitions => m_AchievementDefinitions;

        private readonly Dictionary<string, AchievementProgressState<T>> m_Achievements = new();
        private bool m_Disposed;

        private Task m_FetchNativeDataTask;

        /// <param name="achievements">List of all achievements.</param>
        /// <param name="timeBetweenUpdatesMilliseconds">Time between achievement update events.</param>
        /// <param name="parentLifetimeToken">A lifetime token which when disposed, disables the achievement system.</param>
        protected AbstractAchievementSystem(IReadOnlyList<T> achievements, int timeBetweenUpdatesMilliseconds = k_DefaultTimeBetweenUpdatesMilliseconds, ILifetimeToken parentLifetimeToken = null)
        {
            m_AchievementDefinitions = achievements ?? throw new ArgumentNullException(nameof(achievements));
            m_ParentLifetimeToken = parentLifetimeToken;
            m_TimeBetweenUpdatesMilliseconds = timeBetweenUpdatesMilliseconds;

            PopulateAchievements();

            UpdateLoop();
        }

        private void PopulateAchievements()
        {
            foreach (var achievement in m_AchievementDefinitions)
            {
                m_Achievements[achievement.Id] = new AchievementProgressState<T>(achievement);
            }
        }

        /// <summary>
        /// Triggers initialization of the achievement system on a background thread. Once the initialization completes,
        /// the achievement update loop will start to process achievement state updates.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this is called on an already-running achievement system.</exception>
        public async Task Initialize()
        {
            if (!m_DisabledStop.Stopped)
                throw new InvalidOperationException("Can't run initialize on a running achievement system");

            await InitializeSystem();

            m_DisabledStop.Continue();
            m_FetchNativeDataTask = FetchNativeData();
        }

        private async Task FetchNativeData()
        {
            using var lck = await m_UpdateLock.LockAsync().ConfigureAwait(false);
            try
            {
                await FetchNativeAchievementData();
            }
            finally
            {
                m_InitializationSemaphore.Release();
            }
        }

        /// <summary>
        /// This method should check if there are any easy checks to see if the achievement system will fail to load.
        /// </summary>
        /// <returns>A Task that completes when initialization completes.</returns>
        protected abstract Task InitializeSystem();

        /// <summary>
        /// Fetches the achievement data from the console, and maps it with PT Achievement data
        /// </summary>
        /// <returns></returns>
        protected abstract Task FetchNativeAchievementData();

        public async Task ForceReinitialize()
        {
            // Let any previous initializations complete to avoid competing updates.
            await m_InitializationSemaphore.WaitAsync();

            // Disable the system before grabbing the lock, so the update loop stops before it attempt to grab the lock.
            m_DisabledStop.Stop();
            // Then grab the lock to ensure that the update loop is finished any updates before we start re-initializing.
            using var lck = await m_UpdateLock.LockAsync().ConfigureAwait(false);
            m_Achievements.Clear();
            PopulateAchievements();
            // Note that Initialize will start the main update loop once its background work completes.
            await Initialize();
        }

        public void Unlock(string id)
        {
            UpdateProgress(id, int.MaxValue);
        }

        public void UpdateProgress(string id, int progress)
        {
            m_ParentLifetimeToken?.ThrowOnDisposedAccess();

            if (!m_Achievements.TryGetValue(id, out var achievement))
                throw new ArgumentException($"No achievement with id {id} found.");

            if (progress > achievement.Definition.ProgressTarget)
                progress = achievement.Definition.ProgressTarget;
            if (!achievement.IsProgressRelevant(progress))
                return;

            m_NewUpdateCommands.Enqueue(new AchievementUpdateCommand<T>(achievement, progress));
            m_InactivityStop.Continue();
        }

        /// <summary>
        /// Set progress on the platform side. Updates that are smaller than the ones set,
        /// will not be propagated.
        /// </summary>
        public void SetNativeProgress(string id, int progress, int nativeProgressUpperBound)
        {
            Assert.IsTrue(nativeProgressUpperBound > 0);

            if (!m_Achievements.TryGetValue(id, out var achievement))
                throw new ArgumentException($"No achievement with id {id} found.");

            if (progress >= nativeProgressUpperBound)
            {
                achievement.Unlocked = true;
                return;
            }

            var gradation = (double)achievement.Definition.ProgressTarget / nativeProgressUpperBound;
            var newProgress = (int)Math.Ceiling(gradation * (progress + 1));

            var setProgressCompleted = false;
            while (!setProgressCompleted)
            {
                var currentProgress = achievement.NextProgress;
                if (newProgress <= achievement.NextProgress ||
                    Interlocked.CompareExchange(ref achievement.NextProgress, newProgress, currentProgress) ==
                    currentProgress)
                {
                    setProgressCompleted = true;
                }
            }
        }

        /// <summary>
        /// Mark achievement as non progressive. If achievement is progressive in PT,
        /// no updates will be propagated, except for the unlock.
        /// </summary>
        public void SetNativeNonProgressive(string id)
        {
            if (!m_Achievements.TryGetValue(id, out var achievement))
                throw new ArgumentException($"No achievement with id {id} found.");
            achievement.NextProgress = achievement.Definition.ProgressTarget;
        }

        /// <summary>
        /// Mark achievement as unlocked. No updates will be propagated.
        /// </summary>
        public void SetNativeUnlocked(string id)
        {
            if (!m_Achievements.TryGetValue(id, out var achievement))
                throw new ArgumentException($"No achievement with id {id} found.");
            achievement.Unlocked = true;
        }

        /// <summary>
        /// Mark achievement as invalid. No updates will be propagated. Use this when
        /// an achievement does not exist on the platform side.
        /// </summary>
        public void SetNativeInvalid(string id)
        {
            if (!m_Achievements.TryGetValue(id, out var achievement))
                throw new ArgumentException($"No achievement with id {id} found.");
            achievement.Invalid = true;
        }

        private async void UpdateLoop()
        {
            while (!m_Disposed)
            {
                await m_InactivityStop.WhileStopped();
                m_InactivityStop.Stop();
                do
                {
                    await m_DisabledStop.WhileStopped();
                    using (var lck = await m_UpdateLock.LockAsync().ConfigureAwait(false))
                    {
                        if (m_Disposed)
                            break;
                        AddNewUpdateCommands();
                        m_LastUpdateTime = DateTime.Now;
                        await DoUpdates(1);
                    }

                    var delay = m_TimeBetweenUpdatesMilliseconds - (DateTime.Now - m_LastUpdateTime).Milliseconds;
                    if (delay > 0)
                        await Task.Delay(delay).ConfigureAwait(false);
                } while (m_PendingUpdateCommands.Count > 0);
            }
        }

        /// <summary>
        /// Flush pending updates.
        /// </summary>
        /// <param name="maxUpdates">Maximum number of updates to flush. To flush all updates set to -1.</param>
        public async Task Flush(int maxUpdates = -1)
        {
            using var lck = await m_UpdateLock.LockAsync().ConfigureAwait(false);
            AddNewUpdateCommands();
            m_LastUpdateTime = DateTime.Now;
            await DoUpdates(maxUpdates);
        }

        private async Task DoUpdates(int maxUpdates)
        {
            var pendingUpdates = maxUpdates;
            if (maxUpdates < 0 || maxUpdates > m_PendingUpdateCommands.Count)
                pendingUpdates = m_PendingUpdateCommands.Count;

            var updateTasks = new List<Task>();

            while (pendingUpdates > 0 && m_PendingUpdateCommands.Count > 0)
            {
                var minTime = m_PendingUpdateCommands.Min(c => c.Value.TimeAdded);
                var updateCommand = m_PendingUpdateCommands.First(c => c.Value.TimeAdded == minTime).Value;

                if (updateCommand.IsCommandRelevant())
                {
                    var updateTask = DoUpdate(updateCommand.Achievement.Definition, updateCommand.Progress);
                    updateTasks.Add(updateTask);
                    pendingUpdates--;
                }

                m_PendingUpdateCommands.Remove(updateCommand.Achievement.Definition.Id);
            }

            foreach (var updateTask in updateTasks)
            {
                try
                {
                    await updateTask.ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void AddNewUpdateCommands()
        {
            while (m_NewUpdateCommands.TryDequeue(out var newUpdateCommand))
            {
                var achievementId = newUpdateCommand.Achievement.Definition.Id;
                if (m_PendingUpdateCommands.TryGetValue(achievementId, out var pendingUpdateCommand))
                {
                    pendingUpdateCommand.Progress = newUpdateCommand.Progress;
                }
                else
                {
                    newUpdateCommand.SetTimeAdded();
                    m_PendingUpdateCommands.Add(achievementId, newUpdateCommand);
                }
            }
        }

        /// <summary>
        /// Platform achievement update event. This method must not throw any exceptions. If update fails, but can be retried,
        /// do not retry immediately. Instead call <see cref="UpdateProgress"/> to put the update back into the queue.
        /// </summary>
        /// <param name="achievement">Achievement which progress to update.</param>
        /// <param name="progress">Progress to set. Progress is in PT achievement range.</param>
        /// <returns></returns>
        protected abstract Task DoUpdate(T achievement, int progress);

        public virtual ValueTask DisposeAsync()
        {
            m_Disposed = true;
            m_DisabledStop.Dispose();
            m_InactivityStop.Dispose();
            return new ValueTask();
        }

        protected void LogTypeMismatchPtSingleNativeProgress(T achievement)
        {
            Debug.LogError($"Achievement {achievement.Id} type mismatch. Achievement is set as non-progressive in Platform Toolkit and progressive natively.");
        }

        protected void LogTypeMismatchPtProgressNativeSingle(T achievement)
        {
            Debug.LogError($"Achievement {achievement.Id} type mismatch. Achievement is set as progressive in Platform Toolkit and non-progressive natively.");
        }

        protected void LogProgressTargetMismatch(T achievement, long nativeTarget)
        {
            Debug.LogError($"Achievement {achievement.Id} progress target mismatch. Native target: {nativeTarget}, Platform Toolkit target: {achievement.ProgressTarget}. Updating progress will not work as expected.");
        }

        protected void LogNativeAchievementNotDefined(T achievement)
        {
            Debug.LogError($"Achievement {achievement.Id} is not defined natively.");
        }
    }
}
