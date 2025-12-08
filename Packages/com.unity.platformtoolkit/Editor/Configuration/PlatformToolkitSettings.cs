using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    [FilePath("ProjectSettings/PlatformToolkit.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class PlatformToolkitSettings : ScriptableSingleton<PlatformToolkitSettings>
    {
        [SerializeField]
        public SupportDeclarationTargetsManager SupportDeclarationTargetsManager = new();

        [SerializeField]
        public ObservableSerializableList<StoredAchievement> StoredAchievements = new();
        private StoredAchievementObserver m_StoredAchievementObserver;

        [SerializeField]
        public StoredSettings StoredSettings = new ();
        private Dictionary<string, ISettingsConfiguration> m_SettingsConfigurations = new();

        public static event Action OnStoredAchievementsChanged;

        private void OnEnable()
        {
            SupportDeclarationTargetsManager.SetSupportDeclarations(SupportDeclarationManager.SupportDeclarations);
            EditorApplication.delayCall += Save;
            SupportDeclarationTargetsManager.SupportDeclarationTargetChanged += Save;

            m_StoredAchievementObserver = new (StoredAchievements);

            m_StoredAchievementObserver.AchievementsChanged += StoredAchievementsChanged;
            StoredSettings.SettingsChanged += Save;
        }

        private void OnDisable()
        {
            SupportDeclarationTargetsManager.SupportDeclarationTargetChanged -= Save;
            m_StoredAchievementObserver.AchievementsChanged -= StoredAchievementsChanged;
            StoredSettings.SettingsChanged -= Save;
        }

        private void StoredAchievementsChanged()
        {

            OnStoredAchievementsChanged?.Invoke();
            Save();
        }

        public ISettingsConfiguration GetSettingsConfiguration(string implementationKey)
        {
            if (m_SettingsConfigurations.TryGetValue(implementationKey, out var configuration))
            {
                return configuration;
            }

            if (!SupportDeclarationManager.TryGetSupportDeclaration(implementationKey, out var supportDeclaration))
            {
                throw new ArgumentException($"No support declaration found for key {implementationKey}");
            }

            if (supportDeclaration.SettingsProvider is null)
            {
                return null;
            }

            var configurationContext = new SettingsConfigurationContext(supportDeclaration.Key, StoredSettings);
            configuration = supportDeclaration.SettingsProvider.CreateSettingsConfiguration(configurationContext);

            m_SettingsConfigurations.Add(implementationKey, configuration);
            return configuration;
        }

        private void Save()
        {
            Save(true);
        }
    }
}
