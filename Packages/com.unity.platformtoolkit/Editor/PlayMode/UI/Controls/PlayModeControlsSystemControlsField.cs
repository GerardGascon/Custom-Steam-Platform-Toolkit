using System;
using UnityEngine.UIElements;
using System.Linq;

namespace Unity.PlatformToolkit.PlayMode
{
    [UxmlElement]
    internal partial class PlayModeControlsSystemControlsField : VisualElement
    {
        private readonly int[] k_TimeDelayChoices = { 1, 3, 5 };

        private PlayModeControlsViewModel m_PlayModeControlsView;

        private Toggle m_FullStorage;
        private Toggle m_OfflineNetwork;
        private Toggle m_DelayApiTasks;
        private DropdownField m_TimeDelay;

        public void Bind(PlayModeControlsViewModel playModeControlsView)
        {
            m_PlayModeControlsView = playModeControlsView;

            m_FullStorage ??= this.Q<Toggle>("full-storage-toggle");
            m_OfflineNetwork ??= this.Q<Toggle>("offline-network-toggle");
            m_DelayApiTasks ??= this.Q<Toggle>("long-run-toggle");
            m_TimeDelay ??= this.Q<DropdownField>("time-delay-dropdown");
            m_TimeDelay.choices = k_TimeDelayChoices.Select(x => x.ToString()).ToList();

            m_PlayModeControlsView.OnEnvironmentControlsInvalidated.AddWeakListener(Refresh);

            Refresh();
        }

        private void Refresh()
        {
            m_FullStorage.SetValueWithoutNotify(m_PlayModeControlsView.FullStorage);
            m_OfflineNetwork.SetValueWithoutNotify(m_PlayModeControlsView.OfflineNetwork);
            if (m_PlayModeControlsView.CallsPausingTime.Seconds > 0)
            {
                m_DelayApiTasks.SetValueWithoutNotify(true);
                m_TimeDelay.SetValueWithoutNotify(m_PlayModeControlsView.CallsPausingTime.Seconds.ToString());
            }
            else
            {
                m_DelayApiTasks.SetValueWithoutNotify(false);
                m_TimeDelay.index = 0;
            }

            UnbindToggleCallbacks();

            m_FullStorage.RegisterValueChangedCallback(OnFullStorageToggleChanged);
            m_OfflineNetwork.RegisterValueChangedCallback(OnOfflineNetworkToggleChanged);
            m_DelayApiTasks.RegisterValueChangedCallback(OnDelayApiTasksToggleChanged);
            m_TimeDelay.RegisterValueChangedCallback(OnTimeDelayDropdownChanged);
        }

        private void UnbindToggleCallbacks()
        {
            m_FullStorage.UnregisterValueChangedCallback(OnFullStorageToggleChanged);
            m_OfflineNetwork.UnregisterValueChangedCallback(OnOfflineNetworkToggleChanged);
            m_DelayApiTasks.UnregisterValueChangedCallback(OnDelayApiTasksToggleChanged);
            m_TimeDelay.UnregisterValueChangedCallback(OnTimeDelayDropdownChanged);
        }

        public void Unbind()
        {
            if (m_PlayModeControlsView is null)
                return;

            m_PlayModeControlsView?.OnEnvironmentControlsInvalidated.RemoveListener(Refresh);
            UnbindToggleCallbacks();

            m_PlayModeControlsView = null;
        }

        private void OnFullStorageToggleChanged(ChangeEvent<bool> evt)
        {
            m_PlayModeControlsView.FullStorage = evt.newValue;
        }

        private void OnOfflineNetworkToggleChanged(ChangeEvent<bool> evt)
        {
            m_PlayModeControlsView.OfflineNetwork = evt.newValue;
        }

        private void OnDelayApiTasksToggleChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
                m_PlayModeControlsView.CallsPausingTime = TimeSpan.FromSeconds(k_TimeDelayChoices[m_TimeDelay.index]);
            else
                m_PlayModeControlsView.CallsPausingTime = TimeSpan.Zero;
        }

        private void OnTimeDelayDropdownChanged(ChangeEvent<string> _)
        {
            if (m_DelayApiTasks.value)
                m_PlayModeControlsView.CallsPausingTime = TimeSpan.FromSeconds(k_TimeDelayChoices[m_TimeDelay.index]);
        }
    }
}
