using System;
using System.Collections.Generic;
using System.Linq;
using Unity.PlatformToolkit.Editor;
using Unity.Properties;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Stored achievement state for a play mode account.
    /// </summary>
    [Serializable]
    internal class PlayModeAccountAchievementData : IDisposable
    {
        [SerializeField]
        internal List<PlayModeAchievementData> m_Achievements = new();

        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the PlayModeAccountData that owns this object.
        private ScriptableObjectDataChangePersistor m_Persistor;

        internal ScriptableObjectDataChangePersistor Persistor
        {
            get => m_Persistor;
            set
            {
                m_Persistor = value;
                foreach (var achievement in m_Achievements)
                {
                    achievement.Persistor = value;
                }
            }
        }

        public void Initialize()
        {
            PlatformToolkitSettings.OnStoredAchievementsChanged += UpdateAchievementList;
        }

        public void Dispose()
        {
            PlatformToolkitSettings.OnStoredAchievementsChanged -= UpdateAchievementList;
        }

        [CreateProperty]
        public IReadOnlyList<PlayModeAchievementData> Achievements => m_Achievements;

        public PlayModeAchievementData GetAchievement(string name)
        {
            var achievement = m_Achievements.FirstOrDefault(achievement => achievement.Name == name);
            if (achievement == null)
                throw new ArgumentException($"No achievement named {name}");

            return achievement;
        }

        public void UpdateAchievementList()
        {
            // Note that this data is not staged or double buffered between play and edit mode currently, which means the editor view needs locking to avoid data changing.

            var storedAchievements = PlatformToolkitSettings.instance.StoredAchievements;

            var updatedAchievements = new List<PlayModeAchievementData>(storedAchievements.Count);
            updatedAchievements.AddRange(Enumerable.Repeat<PlayModeAchievementData>(null, storedAchievements.Count));

            var copiedEntryFlags = new bool[m_Achievements.Count];

            bool dirty = storedAchievements.Count != m_Achievements.Count;

            // Iterate source achievements and reorder and update serialized elements as needed.
            // Reordering is done to keep consistency across visual views.
            for (int storedAchievementIndex = 0; storedAchievementIndex < storedAchievements.Count; ++storedAchievementIndex)
            {
                var storedAchievement = storedAchievements[storedAchievementIndex];

                // Find existing entry in the serialized progress data and store it at the matching index of the source list.
                // Handles duplicates, and all elements are eventually filled.
                int searchStartIndex = 0;
                int indexOfExisting = -1;
                while (searchStartIndex == 0 || (indexOfExisting != -1 && copiedEntryFlags[indexOfExisting]))
                {
                    // Find the next un-copied entry in the pre-sorted list
                    indexOfExisting = m_Achievements.FindIndex(searchStartIndex, a => String.Equals(storedAchievement.Id, a.Name));
                    searchStartIndex = (indexOfExisting != -1) ? indexOfExisting + 1 : m_Achievements.Count + 1;
                }

                if (indexOfExisting == -1)
                {
                    // Add a new achievement since it was never found in the existing list.
                    dirty = true;
                    updatedAchievements[storedAchievementIndex] = new PlayModeAchievementData(storedAchievement) { Persistor = m_Persistor };
                }
                else
                {
                    var existingAchievement = m_Achievements[indexOfExisting];
                    copiedEntryFlags[indexOfExisting] = true;

                    dirty |= storedAchievementIndex != indexOfExisting;
                    dirty |= existingAchievement.SyncIfNeeded(storedAchievement);
                    updatedAchievements[storedAchievementIndex] = existingAchievement;
                }
            }
            // This means we have a bug in the fill code
            foreach (var a in updatedAchievements)
                Debug.Assert(a != null, "Null entry in updatedAchievements.");

            if (dirty)
            {
                // Assign the list back. Any unrecognized achievements from m_Achievements are dropped.
                m_Achievements = updatedAchievements;
                Persistor.PersistWrites();
            }
        }
    }
}
