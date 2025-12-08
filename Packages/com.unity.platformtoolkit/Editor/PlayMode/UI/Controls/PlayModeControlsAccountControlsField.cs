using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    [UxmlElement]
    internal partial class PlayModeControlsAccountControlsField : VisualElement
    {
        private PlayModeControlsViewModel m_PlayModeControlsView;

        private VisualElement m_MainContainer;
        private VisualElement m_AccountsList;
        private Button m_RefuseButton;
        private VisualElement m_WarningsContainer;
        private HelpBox m_SelectAccountWarning;
        private HelpBox m_AddAccountError;
        private HelpBox m_NoPrimaryError;

        private bool m_IsBound;

        public void Bind(PlayModeControlsViewModel playModeControlsView)
        {
            Debug.Assert(m_IsBound, "Bind was called without unbinding previous data.");
            m_IsBound = true;

            m_MainContainer ??= this.Q<VisualElement>("main-container");
            m_AccountsList ??= this.Q<VisualElement>("accounts-list");
            m_RefuseButton ??= this.Q<Button>("refuse-button");
            m_WarningsContainer ??= this.Q<VisualElement>("warnings");
            m_SelectAccountWarning ??= this.Q<HelpBox>("select-account-warning");
            m_AddAccountError ??= this.Q<HelpBox>("add-account-error");
            m_NoPrimaryError ??= this.Q<HelpBox>("no-primary-error");

            m_PlayModeControlsView = playModeControlsView;

            m_PlayModeControlsView.OnAccountControlsInvalidated.AddWeakListener(Refresh);

            m_RefuseButton.clicked += RefuseAccountPickRequest;

            Refresh();
        }

        public void Unbind()
        {
            if (m_PlayModeControlsView == null)
                return;

            m_PlayModeControlsView.OnAccountControlsInvalidated.RemoveListener(Refresh);

            m_RefuseButton.clicked -= RefuseAccountPickRequest;

            m_PlayModeControlsView = null;
            m_IsBound = false;
        }

        private void Refresh()
        {
            m_AccountsList.Clear();
            foreach (PlayModeAccountData accountData in m_PlayModeControlsView.AccountData)
            {
                m_AccountsList.Add(new PlayModeControlsAccountField(accountData, m_PlayModeControlsView));
            }

            var requestActive = m_PlayModeControlsView.IsPickAccountRequestActive;
            m_MainContainer.EnableInClassList("warning-border", requestActive);
            m_RefuseButton.EnableInClassList("hidden", !requestActive);
            m_SelectAccountWarning.EnableInClassList("hidden", !requestActive);
            m_AddAccountError.EnableInClassList("hidden", m_PlayModeControlsView.AccountData.Count() > 0);
            var showNoPrimaryError = m_PlayModeControlsView.Capability.PrimaryAccountBehaviour == PrimaryAccountBehaviour.AlwaysSignedIn
                && m_PlayModeControlsView.PrimaryAccountData == null;
            m_NoPrimaryError.EnableInClassList("hidden", !showNoPrimaryError);
        }

        private void RefuseAccountPickRequest()
        {
            m_PlayModeControlsView.RefusedAccountSelection();
        }
    }
}
