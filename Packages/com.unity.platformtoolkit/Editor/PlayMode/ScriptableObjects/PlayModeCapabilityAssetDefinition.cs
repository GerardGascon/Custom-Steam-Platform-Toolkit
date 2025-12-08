using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    #if ENABLE_PSA_VALIDATION_CAPABILITY_EDIT
    [CreateAssetMenu(fileName = "PlayModeCapabilityAssetDefinition", menuName = "Platform Toolkit/Play Mode Capability Definition")]
    #endif
    internal class PlayModeCapabilityAssetDefinition : ScriptableObject, IPlayModeCapability
    {
        [HideInInspector, SerializeField] private string m_Title = "EMPTY - Capability Title Not Set";
        [HideInInspector, SerializeField] private int m_MaxSignedInAccounts;
        [HideInInspector, SerializeField] private PrimaryAccountBehaviour m_PrimaryAccountBehaviour;
        [HideInInspector, SerializeField] private AdditionalAccountBehaviour m_AdditionalAccountBehaviour;
        [HideInInspector, SerializeField] private bool m_AllowMultipleSignInAttempts;
        [HideInInspector, SerializeField] private bool m_SupportsAchievements;
        [HideInInspector, SerializeField] private bool m_AccountsCanManuallySignOut;
        [HideInInspector, SerializeField] private bool m_SupportsAccountInputPairing;
        [HideInInspector, SerializeField] private bool m_SupportsLocalSaving;


        public string Title => m_Title;
        public int MaxSignedInAccounts => m_MaxSignedInAccounts;
        public PrimaryAccountBehaviour PrimaryAccountBehaviour => m_PrimaryAccountBehaviour;
        public AdditionalAccountBehaviour AdditionalAccountBehaviour => m_AdditionalAccountBehaviour;
        public bool AllowMultipleSignInAttempts => m_AllowMultipleSignInAttempts;
        public bool SupportsAchievements => m_SupportsAchievements;
        public bool AccountsCanManuallySignOut => m_AccountsCanManuallySignOut;
        public bool SupportsAccountInputOwnership => m_SupportsAccountInputPairing;
        public bool SupportsLocalSaving => m_SupportsLocalSaving;
    }
}
