using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeControlsAccountDataField : VisualElement
    {
        private PlayModeControlsAccountAchievementsList m_AchievementsList;
        private PlayModeControlsSaveDataField m_SavesList;

        public PlayModeControlsAccountDataField(PlayModeControlsViewModel playModeControlsView)
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.unity.platformtoolkit/Editor/Playmode/UI/Inspector/PlayModeControlsAccountDataField.uxml");
            uxml.CloneTree(this);
            m_AchievementsList = this.Q<PlayModeControlsAccountAchievementsList>();
            m_SavesList = this.Q<PlayModeControlsSaveDataField>();

            m_SavesList.Init(playModeControlsView, isPerAccountSave: true);
        }

        public void Bind(PlayModeAccountData accountData)
        {
            dataSource = accountData;
            m_AchievementsList.Bind(accountData.Achievements);
            m_SavesList.Bind(accountData.Saves, "Saves");
        }
    }
}
