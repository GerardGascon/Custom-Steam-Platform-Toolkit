using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
#if INPUT_SYSTEM_AVAILABLE
using UnityEngine.InputSystem;
#endif // INPUT_SYSTEM_AVAILABLE

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// The UI field (in the Play Mode Controls window) used to pair devices with users
    /// </summary>
    [UxmlElement]
    internal partial class PlayModeControlsInputMappingField : VisualElement
    {
#if INPUT_SYSTEM_AVAILABLE
        private const string k_UnassignedOption = "Unassigned";

        private VisualElement m_UserInputPairsContainer;

        private PlayModeControlsViewModel m_PlayModeControlsViewModel;


        public PlayModeControlsInputMappingField()
        {
            AddToClassList("mb-20");

            var title = new Label("Input Device Mapping");
            title.AddToClassList("title");
            Add(title);
            m_UserInputPairsContainer = new VisualElement();
            Add(m_UserInputPairsContainer);
        }

        public void Bind(PlayModeControlsViewModel playModeControlsView)
        {
            m_PlayModeControlsViewModel = playModeControlsView;
            m_PlayModeControlsViewModel.OnAccountControlsInvalidated.AddWeakListener(Refresh);

            Refresh();
        }

        public void Unbind()
        {
            if (m_PlayModeControlsViewModel == null)
                return;

            m_PlayModeControlsViewModel.OnAccountControlsInvalidated.RemoveListener(Refresh);
            m_PlayModeControlsViewModel = null;
        }

        private void OnPairingChanged()
        {
            Refresh();
        }

        private void OnAccountStateChanged(PlayModeAccountData accountData, AccountState state)
        {
            //We refresh as the dropdown options need to update whenever an account state changes
            Refresh();
        }

        private void Refresh()
        {
            m_UserInputPairsContainer.Clear();

            var signedInAccounts = m_PlayModeControlsViewModel.SignedInAccounts;

            var options = new List<string> { k_UnassignedOption };
            options.AddRange(signedInAccounts.Select(account => account.PublicName));

            //Ensuring the accounts list matches the dropdown options
            var accounts = new List<PlayModeAccountData>();
            //null represents the unassigned option in the dropdown
            accounts.Add(null);
            accounts.AddRange(signedInAccounts);

            if (!m_PlayModeControlsViewModel.SupportAccountOwnership)
                return;

            var devicePairs = m_PlayModeControlsViewModel.GetAccountDevicePairs();

            foreach (var (device, account) in devicePairs)
            {
                var container = new VisualElement();
                container.Add(new Label(device.name));

                var dropdown = new DropdownField(options, accounts.IndexOf(account));
                dropdown.RegisterValueChangedCallback((changeEvent) =>
                {
                    var newAccount = accounts[dropdown.index];

                    if (newAccount == null)
                        m_PlayModeControlsViewModel.UnassignInputDevice(device.deviceId);
                    else
                        m_PlayModeControlsViewModel.AssignInputDevice(device.deviceId, newAccount);
                });

                container.Add(dropdown);
                container.AddToClassList("row");
                container.AddToClassList("justify");

                m_UserInputPairsContainer.Add(container);
            }
        }
#endif // INPUT_SYSTEM_AVAILABLE
    }
}
