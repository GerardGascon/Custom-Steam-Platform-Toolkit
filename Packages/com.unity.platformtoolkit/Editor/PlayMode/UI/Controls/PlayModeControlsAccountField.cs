using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    [UxmlElement]
    internal partial class PlayModeControlsAccountField : VisualElement
    {
        private readonly IReadOnlyDictionary<SetPrimaryAccountStatus, string> k_SetPrimaryAccountStatusTooltips = new Dictionary<SetPrimaryAccountStatus, string>
            {
                { SetPrimaryAccountStatus.CannotSignIn, "This account cannot currently be signed in, hence it cannot be set as primary." },
                { SetPrimaryAccountStatus.NotSupported, "This platform does not support the concept of primary accounts." },
                { SetPrimaryAccountStatus.NotSupportedInPlaymode, "Setting the primary account is only allowed outside of Play Mode on this platform." }
            };

        private readonly IReadOnlyDictionary<SignInStatus, string> k_SignInStateTooltips = new Dictionary<SignInStatus, string>()
            {
                { SignInStatus.AdditionalAccountsNotSupported, "This platform only supports one signed-in account at a time." },
                { SignInStatus.MaximumAccountsReached, "The maximum number of supported signed-in accounts for this platform has been reached." },
                { SignInStatus.OnlyAllowedWithRequests, "Accounts can only sign-in on this platform in response to a Platform Toolkit API request." }
            };

        private readonly IReadOnlyDictionary<SignOutStatus, string> k_SignOutStatusTooltips = new Dictionary<SignOutStatus, string>
            {
                { SignOutStatus.CannotSignOutPrimaryInPlayMode, "The primary account cannot be signed out while in Play Mode on this platform." }
            };

        public PlayModeControlsAccountField() {}

        public PlayModeControlsAccountField(PlayModeAccountData accountData, PlayModeControlsViewModel playModeView)
        {
            if (!playModeView.IsValid)
                return;

            AddToClassList("row");

            var isPrimaryAccount = playModeView.PrimaryAccountData == accountData;
            VisualElement isPrimary;
            if (playModeView.IsPlaying)
                isPrimary = new Label(isPrimaryAccount ? "👑" : "");
            else
            {
                var radioButton = new RadioButton();
                radioButton.SetValueWithoutNotify(isPrimaryAccount);

                var canSetToPrimary = playModeView.CanSetAccountToPrimaryManually(accountData);
                if (canSetToPrimary != SetPrimaryAccountStatus.Allowed)
                {
                    radioButton.SetEnabled(false);
                    radioButton.tooltip = k_SetPrimaryAccountStatusTooltips[canSetToPrimary];
                }

                radioButton.RegisterValueChangedCallback(_ =>
                {
                    if (radioButton.value)
                        playModeView.SetToPrimary(accountData);
                });

                isPrimary = radioButton;
            }
            isPrimary.AddToClassList("wide");

            var accountNameLabel = new Label(accountData.PublicName);
            accountNameLabel.AddToClassList("extra-wide");
            accountNameLabel.AddToClassList("truncated-text");

            var canSignInManually = playModeView.CanSignInAccountManually(accountData);
            var isSignedIn = playModeView.IsAccountSignedIn(accountData);
            //The toggle is wrapped in a container (VisualElement) because the tooltip does not seem to work on Toggles directly
            var isSignedInToggleContainer = new VisualElement();
            var isSignedInToggle = new Toggle();
            isSignedInToggleContainer.Add(isSignedInToggle);
            isSignedInToggle.AddToClassList("wide");
            isSignedInToggle.SetValueWithoutNotify(isSignedIn);
            if (isSignedIn)
            {
                var canSignOut = playModeView.CanSignOutAccount(accountData);
                if (canSignOut != SignOutStatus.Allowed)
                {
                    isSignedInToggle.SetEnabled(false);
                    isSignedInToggleContainer.tooltip = k_SignOutStatusTooltips[canSignOut];
                }
            }
            else if (!isSignedIn)
            {

                if (canSignInManually != SignInStatus.Allowed)
                {
                    isSignedInToggle.SetEnabled(false);
                    isSignedInToggleContainer.tooltip = k_SignInStateTooltips[canSignInManually];
                }
            }
            isSignedInToggle.RegisterValueChangedCallback(_ =>
            {
                if (isSignedInToggle.value)
                    playModeView.SignInAccount(accountData);
                else
                    playModeView.SignOutAccount(accountData);
            });

            var canSignInWithRequest = playModeView.CanSignInAccount(accountData);
            var selectAccount = new Button();
            selectAccount.text = "Select";
            selectAccount.AddToClassList("bold");
            selectAccount.AddToClassList("wide");
            selectAccount.SetEnabled(false);
            switch(canSignInWithRequest)
            {
                case SignInStatus.AdditionalAccountsNotSupported:
                    selectAccount.tooltip = "This account cannot be selected and signed in because this platform does not support additional accounts.";
                    break;
                case SignInStatus.MaximumAccountsReached:
                    selectAccount.tooltip = "This account cannot be selected and signed in because the maximum number of supported signed-in accounts for this platform has been reached.";
                    break;
                default:
                    selectAccount.SetEnabled(true);
                    break;
            }

            selectAccount.clicked += () =>
            {
                playModeView.OnAccountPicked(accountData);
            };

            if (!playModeView.IsPickAccountRequestActive)
                selectAccount.AddToClassList("hidden");

            Add(isPrimary);
            Add(accountNameLabel);
            Add(isSignedInToggleContainer);
            Add(selectAccount);
        }
    }
}
