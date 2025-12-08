using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Editor UI (part of a custom Inspector) for play mode account achievement state.
    /// </summary>
    [UxmlElement]
    internal partial class PlayModeControlsAccountAchievementsList : VisualElement
    {
        public PlayModeControlsAccountAchievementsList()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.platformtoolkit/Editor/Playmode/UI/Inspector/PlayModeControlsAccountAchievementsList.uxml");
            uxml.CloneTree(this);
        }

        internal void Bind(PlayModeAccountAchievementData achievementData)
        {
            achievementData.UpdateAchievementList();

            var achievementsList = this.Q<ListView>("achievements-list");
            achievementsList.makeItem = () => new PlayModeControlsAchievementField();
            achievementsList.bindItem = (achievementField, index) => achievementField.dataSource = achievementData.Achievements[index];
            achievementsList.dataSource = achievementData;

            var resetButton = this.Q<Button>("reset-all-button");
            resetButton.clicked += () =>
            {
                foreach (var achievement in achievementData.Achievements)
                {
                    achievement.Reset();
                }
            };
        }
    }
}
