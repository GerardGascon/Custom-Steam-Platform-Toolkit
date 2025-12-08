using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class PlatformToolkitProjectSettings
    {
        [SettingsProviderGroup]
        public static SettingsProvider[] CreateProviders()
        {
            var providers = new List<SettingsProvider>();
            providers.Add(new SettingsProvider("Project/PlatformToolkit", SettingsScope.Project)
            {
                label = "Platform Toolkit",
                activateHandler = (_, rootElement) =>
                {
                    CreateSettingsView(out var root, out var title, out var content);

                    title.text = "Platform Toolkit";
                    content.Add(new SupportDeclarationTargetsList());

                    rootElement.Add(root);
                }
            });

            foreach (var supportDeclaration in SupportDeclarationManager.SupportDeclarations)
            {
                if (supportDeclaration.SettingsProvider != null)
                {
                    var settingsConfiguration = PlatformToolkitSettings.instance.GetSettingsConfiguration(supportDeclaration.Key);
                    providers.Add(new SettingsProvider($"Project/PlatformToolkit/{supportDeclaration.DisplayName}", SettingsScope.Project) {
                        label = $"{supportDeclaration.DisplayName}",
                        activateHandler = (_, rootElement) =>
                        {
                            CreateSettingsView(out var root, out var title, out var content);
                            title.text = supportDeclaration.DisplayName;
                            content.Add(settingsConfiguration.CreateSettingsUI());
                            rootElement.Add(root);
                        }
                    });
                }
            }

            return providers.ToArray();
        }

        private static void CreateSettingsView(out VisualElement root, out Label title, out VisualElement content)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/EditorResources/UI/ProjectSettingsContainer.uxml");

            root = visualTreeAsset.Instantiate();
            title = root.Q<Label>("title");
            content = root.Q<VisualElement>("content");
        }
    }
}
