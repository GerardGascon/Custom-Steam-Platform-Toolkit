using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    internal class SupportDeclarationTargetField : VisualElement
    {
        public SupportDeclarationTargetField(SupportDeclarationTarget supportDeclarationTarget)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/EditorResources/UI/SupportDeclarationTargetField.uxml");
            uxml.CloneTree(this);

            var buttonGroup = this.Q<ToggleButtonGroup>("target-build-selector");
            foreach (var supportedBuildTarget in supportDeclarationTarget.SupportedBuildTargets)
            {
                var button = new Button { text = BuildTargetToString(supportedBuildTarget) };
                button.AddToClassList("target-selector__target-group__target-toggle");
                buttonGroup.Add(button);
            }
            dataSource = supportDeclarationTarget;
        }

        private static string BuildTargetToString(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => "Windows x86",
                BuildTarget.StandaloneWindows64 => "Windows x64",
                BuildTarget.StandaloneOSX => "macOS",
                BuildTarget.StandaloneLinux64 => "Linux",
                BuildTarget.Android => "Android™",
                BuildTarget.Switch => "Nintendo Switch™",
                BuildTarget.Switch2 => "Nintendo Switch™ 2",
                BuildTarget.GameCoreXboxSeries => "Xbox Series X|S",
                BuildTarget.GameCoreXboxOne => "Xbox One",
                BuildTarget.PS5 => "PlayStation®5",
                _ => target.ToString()
            };
        }
    }
}
