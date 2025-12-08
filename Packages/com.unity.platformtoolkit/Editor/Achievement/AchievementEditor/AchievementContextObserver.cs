using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class AchievementContextObserver
    {
        public event Action AchievementConfigurationDataChanged;

        private IAchievementConfigurationContext m_Context;

        private HashSet<IAchievement> m_KnownAchievements;

        public AchievementContextObserver(IAchievementConfigurationContext context)
        {
            m_Context = context;
            m_KnownAchievements = new HashSet<IAchievement>(m_Context.Achievements);
            foreach (var achievement in m_KnownAchievements)
            {
                achievement.propertyChanged += AchievementPropertyChanged;
                achievement.ImplementationData.propertyChanged += AchievementPropertyChanged;
            }
            context.AchievementRemoved += OnAchievementRemovedAdded;
            context.AchievementAdded += OnAchievementRemovedAdded;
        }

        private void OnAchievementRemovedAdded(IAchievement targetAchievement)
        {
            var achievements = new HashSet<IAchievement>(m_Context.Achievements);
            if (m_KnownAchievements.SetEquals(achievements))
            {
                return;
            }

            var removedAchievements = new HashSet<IAchievement>(m_KnownAchievements);
            removedAchievements.ExceptWith(m_Context.Achievements);
            foreach (var achievement in removedAchievements)
            {
                achievement.propertyChanged -= AchievementPropertyChanged;
                achievement.ImplementationData.propertyChanged -= AchievementPropertyChanged;
            }

            var addedAchievements = new HashSet<IAchievement>(achievements);
            addedAchievements.ExceptWith(m_KnownAchievements);
            foreach (var achievement in addedAchievements)
            {
                achievement.propertyChanged += AchievementPropertyChanged;
                achievement.ImplementationData.propertyChanged += AchievementPropertyChanged;
            }

            m_KnownAchievements = achievements;
        }

        private void AchievementPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
        {
            AchievementConfigurationDataChanged?.Invoke();
        }
    }
}
