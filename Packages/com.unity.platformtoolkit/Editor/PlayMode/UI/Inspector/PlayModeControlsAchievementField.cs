using Unity.PlatformToolkit.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeControlsAchievementField : VisualElement
    {
        private static readonly ToggleButtonGroupState k_LockedState = new (0x01, 2);
        private static readonly ToggleButtonGroupState k_UnlockedState = new (0x02, 2);

        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            var toggleGroupConverters = new ConverterGroup("Locked Unlocked Toggle Converters");
            toggleGroupConverters.AddConverter((ref bool val) => val ? k_UnlockedState : k_LockedState);
            toggleGroupConverters.AddConverter((ref ToggleButtonGroupState val) => val == k_UnlockedState);

            var progressVisibilityConverters = new ConverterGroup("Progress UI Visibility Converter");
            progressVisibilityConverters.AddConverter((ref UnlockType unlockType) => unlockType == UnlockType.Progressive);

            ConverterGroups.RegisterConverterGroup(toggleGroupConverters);
            ConverterGroups.RegisterConverterGroup(progressVisibilityConverters);
        }

        public PlayModeControlsAchievementField()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/Editor/Playmode/UI/Inspector/PlayModeControlsAchievementField.uxml");
            uxml.CloneTree(this);
        }
    }
}
