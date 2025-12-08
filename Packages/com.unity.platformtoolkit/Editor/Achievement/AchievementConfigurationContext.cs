using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    internal class AchievementConfigurationContext : IAchievementConfigurationContext, IReadOnlyList<IAchievement>
    {
        private readonly ObservableSerializableList<StoredAchievement> m_StoredAchievements;
        private readonly string m_PlatformToolkitId;

        private Dictionary<StoredAchievement, ConfigurationAchievement> m_Achievements = new Dictionary<StoredAchievement, ConfigurationAchievement>();

        public AchievementConfigurationContext(string platformToolkitId, ObservableSerializableList<StoredAchievement> achievements)
        {
            m_PlatformToolkitId = platformToolkitId;
            m_StoredAchievements = achievements;

            m_StoredAchievements.CollectionChanged += OnAchievementCollectionChanged;
        }

        private void OnAchievementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // When achievements are moved there might be some intermediary steps when achievements are repeated in the list. We don't want achievement
            // configurations to deal with this situation, so event is called only after all achievements have been moved. There should never be a situation
            // where the same achievement object is stored multiple times.
            if (e.Action == NotifyCollectionChangedAction.Replace && m_StoredAchievements.Distinct().Count() != m_StoredAchievements.Count)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (StoredAchievement achievement in e.NewItems)
                {
                    AchievementAdded?.Invoke(GetOrCreateAchievement(achievement));
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (StoredAchievement achievement in e.OldItems)
                {
                    AchievementRemoved?.Invoke(GetOrCreateAchievement(achievement));
                }
            }
        }

        public event Action<IAchievement> AchievementRemoved;
        public event Action<IAchievement> AchievementAdded;

        public IReadOnlyList<IAchievement> Achievements => this;

        public IEnumerator<IAchievement> GetEnumerator()
        {
            return m_StoredAchievements.Select(GetOrCreateAchievement).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => m_StoredAchievements.Count;

        public IAchievement this[int index] => GetOrCreateAchievement(m_StoredAchievements[index]);

        private IAchievement GetOrCreateAchievement(StoredAchievement storedAchievement)
        {
            if (m_Achievements.TryGetValue(storedAchievement, out var value))
                return value;

            var newAchievement = new ConfigurationAchievement(m_PlatformToolkitId, storedAchievement);
            m_Achievements[storedAchievement] = newAchievement;
            return newAchievement;
        }
    }
}
