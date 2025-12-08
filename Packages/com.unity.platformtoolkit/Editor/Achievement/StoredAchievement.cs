using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    [Serializable]
    internal class StoredAchievement : ISerializationCallbackReceiver, INotifyBindablePropertyChanged
    {
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        public event Action<object, string> ConfigurationDataChanged;

        [SerializeField]
        [DontCreateProperty]
        private string id = string.Empty;

        [CreateProperty]
        public string Id
        {
            get => id;
            set
            {
                var tempValue = value.Trim();
                if (tempValue.Length > AchievementEditor.AchievementCharacterLimit)
                {
                    Debug.LogWarning($"Character limit exceeded in achievement ID. Limit: {AchievementEditor.AchievementCharacterLimit}.");
                    return;
                }
                if (Regex.IsMatch(tempValue, AchievementEditor.CommonIdRegexPattern) || tempValue.Length == 0)
                {
                    SetProperty(ref id, tempValue);
                }
                else
                {
                    Debug.LogWarning($"Illegal character used in achievement ID. The only allowed characters are: {AchievementEditor.CommonIdRegexPattern}.");
                }
            }
        }

        [SerializeField]
        [DontCreateProperty]
        private UnlockType unlockType;

        [CreateProperty]
        public UnlockType UnlockType
        {
            get => unlockType;
            set => SetProperty(ref unlockType, value);
        }

        [SerializeField]
        [DontCreateProperty]
        private int progressTarget = 1;

        [CreateProperty]
        public int ProgressTarget
        {
            get => progressTarget;
            set
            {
                var valueInRange = value < 1 ? 1 : value;
                SetProperty(ref progressTarget, valueInRange);
            }
        }

        public StoredAchievement Clone()
        {
            var implementationData = new Dictionary<string, ImplementationData>();
            foreach (var keyValuePair in m_ImplementationData)
            {
                implementationData.Add(keyValuePair.Key, new ImplementationData()
                {
                    ConfigurationData = keyValuePair.Value.ConfigurationData,
                    Ignore = keyValuePair.Value.Ignore,
                });
            }
            return new StoredAchievement()
            {
                m_ImplementationData = implementationData,
                Id = this.Id,
                UnlockType = this.UnlockType,
                ProgressTarget = this.ProgressTarget,
            };
        }

        [SerializeField]
        private List<string> implementationKeys;

        [SerializeField]
        private List<ImplementationData> implementationDataValues;

        private Dictionary<string, ImplementationData> m_ImplementationData = new Dictionary<string, ImplementationData>();

        public ImplementationData GetImplementationData(string implementationKey)
        {
            if (m_ImplementationData.TryGetValue(implementationKey, out var data))
            {
                return data;
            }
            else
            {
                var implementationData = new ImplementationData();
                m_ImplementationData.Add(implementationKey, implementationData);
                implementationData.propertyChanged += (sender, args) =>
                {
                    ConfigurationDataChanged?.Invoke(this, implementationKey);
                };
                return implementationData;
            }
        }

        public bool TryGetImplementationData(string implementationKey, out ImplementationData data)
        {
            return m_ImplementationData.TryGetValue(implementationKey, out data);
        }

        public void SetConfigurationData(string implementationKey, string data)
        {
            if (m_ImplementationData.TryGetValue(implementationKey, out var oldData) && oldData.ConfigurationData == data)
                return;

            m_ImplementationData[implementationKey].ConfigurationData = data;
            ConfigurationDataChanged?.Invoke(this, implementationKey);
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string property = "")
        {
            if (value == null && field == null || value != null && value.Equals(field))
                return;

            field = value;
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }

        public void OnBeforeSerialize()
        {
            implementationKeys = new List<string>(m_ImplementationData.Count);
            implementationDataValues = new List<ImplementationData>(m_ImplementationData.Count);

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
                var key = implementationKeys[i];
                implementationDataValues[i].propertyChanged += (sender, args) =>
                {
                    ConfigurationDataChanged?.Invoke(this, key);
                };
                m_ImplementationData[implementationKeys[i]] = implementationDataValues[i];
            }

            implementationKeys = null;
            implementationDataValues = null;
        }
    }
}
