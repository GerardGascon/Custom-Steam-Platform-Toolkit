using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal class StoredSettings : ISerializationCallbackReceiver
    {
        public event Action SettingsChanged;

        [SerializeField]
        private List<string> implementationKeys;

        [SerializeField]
        private List<string> implementationDataValues;

        private Dictionary<string, string> m_ImplementationData = new Dictionary<string, string>();

        public void SetConfigurationData(string implementationKey, string data)
        {
            if (m_ImplementationData.TryGetValue(implementationKey, out var oldData) && oldData == data)
                return;

            m_ImplementationData[implementationKey] = data;
            SettingsChanged?.Invoke();
        }

        public bool TryGetImplementationData(string implementationKey, out string data)
        {
            return m_ImplementationData.TryGetValue(implementationKey, out data);
        }

        public void OnBeforeSerialize()
        {
            implementationKeys = new List<string>(m_ImplementationData.Count);
            implementationDataValues = new List<string>(m_ImplementationData.Count);

            foreach (var implementationData in m_ImplementationData)
            {
                implementationKeys.Add(implementationData.Key);
                implementationDataValues.Add(implementationData.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            for (int i = 0; i < implementationKeys.Count; i++)
            {
                m_ImplementationData[implementationKeys[i]] = implementationDataValues[i];
            }

            implementationKeys = null;
            implementationDataValues = null;
        }
    }
}
