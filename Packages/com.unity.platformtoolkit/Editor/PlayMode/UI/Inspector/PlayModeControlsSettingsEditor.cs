using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    [CustomEditor(typeof(PlayModeControlsSettings))]
    internal class PlayModeControlsSettingsEditor : UnityEditor.Editor, IDisposable
    {
        private VisualElement m_InspectorRoot;

        private PlayModeTestAccountDataView m_TestAccountDataView;
        public override VisualElement CreateInspectorGUI()
        {
            // CreateInspectorGUI is called more than once in some circumstances.
            // Because of this issue, we need to safely dispose the old root before re-creating it again.
            if (m_InspectorRoot != null)
            {
                Dispose();
            }

            m_InspectorRoot = new VisualElement();
            m_TestAccountDataView = new PlayModeTestAccountDataView(showPlatformField: true);
            var settings = (PlayModeControlsSettings)target;
            m_TestAccountDataView.SetPlayModeControlsViewModel(settings.ViewModel);
            m_InspectorRoot.Add(m_TestAccountDataView.BuildView());

            return m_InspectorRoot;
        }

        public void Dispose()
        {
            m_InspectorRoot?.Clear();
        }
    }
}
