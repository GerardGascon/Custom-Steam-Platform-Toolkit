using System;
using Unity.PlatformToolkit.Editor;
using Unity.Properties;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Stored state for a specific achievement.
    /// </summary>
    [Serializable]
    internal class PlayModeAchievementData
    {
        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the PlayModeAccountData that owns this object.
        internal ScriptableObjectDataChangePersistor Persistor;

        [SerializeField] private string m_Name;
        [SerializeField] private bool m_Unlocked;
        [SerializeField] private int m_Progress;

        internal PlayModeAchievementData(StoredAchievement commonAchievement)
        {
            m_Name = commonAchievement.Id;
            Category = commonAchievement.UnlockType;
            TargetValue = commonAchievement.ProgressTarget;
        }

        public void Reset()
        {
            Unlocked = false;
            Progress = 0;
        }

        [CreateProperty]
        public UnlockType Category { get; private set; }

        [CreateProperty]
        public int TargetValue { get; private set; }

        [CreateProperty]
        public string Name
        {
            get => m_Name;
            set
            {
                m_Name = value;
                Persistor.PersistWrites();
            }
        }

        [CreateProperty]
        public int Progress
        {
            get => m_Progress;
            set
            {
                if (m_Progress == value)
                    return;

                SetProgressWithoutNotify(value);
                Persistor.PersistWrites();
            }
        }

        // Sets the progress value without persisting writes / notifying UI about the change.
        // Called by the UI's write actions, so that UI isn't triggering its own recreation while it's being used.
        public void SetProgressWithoutNotify(int value)
        {
            if (Category != UnlockType.Progressive)
                throw new InvalidOperationException("Can't set progress on non-progressive achievements");
            m_Progress = Math.Clamp(value, 0, TargetValue);
            m_Unlocked = (m_Progress == TargetValue);
            Persistor.PersistWritesWithoutNotify();
        }

        [CreateProperty]
        public bool Unlocked
        {
            get => m_Unlocked;
            set
            {
                if (m_Unlocked == value)
                    return;

                m_Unlocked = value;

                if (Category == UnlockType.Progressive)
                    m_Progress = value ? TargetValue : 0;

                Persistor.PersistWrites();
            }
        }

        /// <summary>
        /// Synchronize the achievement with the matching common achievement (if needed).
        /// Doesn't persist writes: relies on the caller to do so.
        /// </summary>
        /// <param name="commonAchievement">The common achievement to synchronize with.</param>
        /// <returns>Whether this achievement was modified.</returns>
        public bool SyncIfNeeded(StoredAchievement commonAchievement)
        {
            var dirty = false;
            if (commonAchievement.Id != Name)
            {
                Name = commonAchievement.Id;
                dirty = true;
            }

            if (commonAchievement.UnlockType != Category)
            {
                Category = commonAchievement.UnlockType;
                dirty = true;
            }

            if (commonAchievement.ProgressTarget != TargetValue)
            {
                TargetValue = commonAchievement.ProgressTarget;
                dirty = true;
                if (m_Unlocked)
                {
                    m_Progress = TargetValue;
                }
                else if (m_Progress >= TargetValue)
                {
                    m_Progress = TargetValue;
                    m_Unlocked = true;
                }
            }

            return dirty;
        }
    }
}
