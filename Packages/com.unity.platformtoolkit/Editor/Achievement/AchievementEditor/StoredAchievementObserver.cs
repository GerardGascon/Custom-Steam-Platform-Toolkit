using System;
using System.Collections.Specialized;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Helper class to group all changes to the achievement data into a single event.</summary>
    internal class StoredAchievementObserver
    {
        public event Action AchievementsChanged;

        private ObservableSerializableList<StoredAchievement> m_StoredAchievements;

        public StoredAchievementObserver(ObservableSerializableList<StoredAchievement> achievements)
        {
            m_StoredAchievements = achievements;
            foreach (var achievement in achievements)
            {
                BindAchievement(achievement);
            }

            achievements.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StoredAchievement newItem in e.NewItems)
                {
                    BindAchievement(newItem);
                }
            }

            if (e.OldItems != null)
            {
                foreach (StoredAchievement oldItems in e.OldItems)
                {
                    UnbindAchievement(oldItems);
                }
            }

            InvokeAchievementsChanged();
        }

        private void InvokeAchievementsChanged()
        {
            AchievementsChanged?.Invoke();
        }

        private void BindAchievement(StoredAchievement achievement)
        {
            achievement.propertyChanged += OnPropertyChanged;
            achievement.ConfigurationDataChanged += OnConfigurationDataChanged;
        }

        private void UnbindAchievement(StoredAchievement achievement)
        {
            achievement.propertyChanged -= OnPropertyChanged;
            achievement.ConfigurationDataChanged -= OnConfigurationDataChanged;
        }

        private void OnConfigurationDataChanged(object sender, string implementationKey)
        {
            InvokeAchievementsChanged();
        }

        private void OnPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            InvokeAchievementsChanged();
        }

    }
}
