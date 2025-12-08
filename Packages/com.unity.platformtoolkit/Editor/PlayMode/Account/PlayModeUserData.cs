using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Serialized data for the Play Mode User Manager.
    /// </summary>
    [Serializable]
    internal class PlayModeUserData
    {
        [SerializeReference] private List<PlayModeAccountData> m_SignedInAccounts = new();

        [SerializeReference] private List<PlayModeAccountData> m_AccountData = new();

        [SerializeReference] private PlayModeAccountData m_PrimaryAccountData;


        public PlayModeUserData(IEnumerable<PlayModeAccountData> testingData)
        {
            m_AccountData.AddRange(testingData);
        }

        public PlayModeUserData() : this(Array.Empty<PlayModeAccountData>())
        {
        }

        internal List<PlayModeAccountData> SignedInAccounts => m_SignedInAccounts;
        internal List<PlayModeAccountData> AccountData => m_AccountData;

        internal PlayModeAccountData PrimaryAccountData
        {
            get => m_PrimaryAccountData;
            set => m_PrimaryAccountData = value;
        }
    }
}
