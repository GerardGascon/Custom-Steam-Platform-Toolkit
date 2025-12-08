using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class ConfigurationAchievement : IAchievement
    {
       [CreateProperty]
        public string Id => m_StoredAchievement.Id;

        [CreateProperty]
        public UnlockType UnlockType => m_StoredAchievement.UnlockType;

        [CreateProperty]
        public int ProgressTarget => m_StoredAchievement.ProgressTarget;

        [CreateProperty]
        public ImplementationData ImplementationData => m_StoredAchievement.GetImplementationData(m_PlatformToolkitId);

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        private readonly string m_PlatformToolkitId;
        private readonly StoredAchievement m_StoredAchievement;

        public ConfigurationAchievement(string platformToolkitId, StoredAchievement achievement)
        {
            m_PlatformToolkitId = platformToolkitId;
            m_StoredAchievement = achievement;

            m_StoredAchievement.propertyChanged += StoredAchievementPropertyChanged;
        }

        private void StoredAchievementPropertyChanged(object _, BindablePropertyChangedEventArgs e)
        {
            propertyChanged?.Invoke(this, e);
        }
    }
}
