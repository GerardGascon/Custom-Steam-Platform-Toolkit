using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Unity.PlatformToolkit.Editor
{
    internal class PlatformToolkitBuilder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => int.MaxValue;

        private IPlatformToolkitBuilder m_Builder;
        private Object m_RuntimeConfiguration;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!PlatformToolkitSettings.instance.SupportDeclarationTargetsManager.TryGetDeclarationForBuildTarget(report.summary.platform, out var declarationKey))
            {
                Debug.LogWarning($"No PT implementation configured for build target {report.summary.platform}");
                return;
            }

            Assert.IsTrue(SupportDeclarationManager.TryGetSupportDeclaration(declarationKey, out var supportDeclaration));

            IAchievementConfigurationContext achievementContext = null;
            if (supportDeclaration.AchievementsSupported)
                achievementContext = new AchievementConfigurationContext(supportDeclaration.Key, PlatformToolkitSettings.instance.StoredAchievements);

            ISettingsConfigurationContext settingsContext = null;
            if (supportDeclaration.SettingsProvider != null)
                settingsContext = new SettingsConfigurationContext(supportDeclaration.Key, PlatformToolkitSettings.instance.StoredSettings);

            m_Builder = supportDeclaration.CreateBuilder(achievementContext, settingsContext);
            if (m_Builder == null)
                return;

            m_RuntimeConfiguration = m_Builder.PrepareBuild(report);

            AssetDatabase.CreateAsset(m_RuntimeConfiguration, "Assets/_PT_BuildBundle.asset");
            AddPreloadedAsset(m_RuntimeConfiguration);

            AssemblyReloadEvents.beforeAssemblyReload += CleanUpProjectSettings;
            CleanUpProjectSettingsAfterBuildEnds();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            m_Builder.PostBuild(report);
        }

        private async void CleanUpProjectSettingsAfterBuildEnds()
        {
            while (BuildPipeline.isBuildingPlayer)
            {
                await Task.Delay(100);
            }
            CleanUpProjectSettings();
        }

        private void CleanUpProjectSettings()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= CleanUpProjectSettings;

            RemovePreloadedAsset(m_RuntimeConfiguration);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_RuntimeConfiguration));
            AssetDatabase.SaveAssets();
        }

        private void AddPreloadedAsset(Object asset)
        {
            if (asset == null)
                return;

            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            if (!preloadedAssets.Contains(asset))
            {
                preloadedAssets.Add(asset);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }

        private void RemovePreloadedAsset(Object asset)
        {
            if (asset == null)
                return;

            var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            if (preloadedAssets.Remove(asset))
            {
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
        }
    }
}
