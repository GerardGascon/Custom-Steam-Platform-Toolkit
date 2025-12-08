using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Stored save state for a play mode system (a user account, or just local saves).
    /// </summary>
    [Serializable]
    internal class PlayModeSaveData
    {
        // Unity can't serialize List<array>, but it can serialize List<non-array>. Wrap the byte[] that will
        // store save data in a custom type so it can be serialized.
        [Serializable]
        private class SerializedSaveData
        {
            public PlayModeSaveDataInfo info;
            public byte[] data = { };
        }

        // Per documentation on SerializableDictionary, we create a subclass in order to serialize its contents.
        [Serializable]
        private class SaveDataKeyedByName : SerializableDictionary<string, SerializedSaveData>
        {
        }

        // Save data contents, keyed by save name.
        [SerializeField]
        private SaveDataKeyedByName m_SaveData = new();

        // Source used for data binding. Name entries are stored in order for correct insertion & removal.
        private List<PlayModeSaveDataInfo> m_BindableSaveData;
        private List<string> m_BindableNameEntries;

        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the PlayModeAccountData that owns this object.
        private ScriptableObjectDataChangePersistor m_Persistor;

        public byte[] ReadSave(string name) => m_SaveData[name].data;

        public PlayModeSaveDataInfo GetSaveInfo(string name) => m_SaveData[name].info;

        public IReadOnlyList<string> GetSaveNames() => m_SaveData.Keys.ToList();

        [CreateProperty]
        public IReadOnlyList<PlayModeSaveDataInfo> SaveData => m_BindableSaveData;

        internal void Initialize(ScriptableObjectDataChangePersistor persistor)
        {
            m_Persistor = persistor;

            m_BindableNameEntries = new List<string>(m_SaveData.Count);
            m_BindableSaveData = new List<PlayModeSaveDataInfo>(m_SaveData.Count);
            foreach (var entry in m_SaveData)
            {
                m_BindableNameEntries.Add(entry.Key);
                m_BindableSaveData.Add(entry.Value.info);
            }
        }

        public void WriteSave(string name, byte[] data, PlayModeSaveDataInfo saveInfo)
        {
            m_SaveData[name] = new SerializedSaveData() { data = data, info = saveInfo};

            int listEntryIndex = m_BindableNameEntries.IndexOf(name);
            if (listEntryIndex != -1)
            {
                m_BindableSaveData[listEntryIndex] = saveInfo;
            }
            else
            {
                m_BindableNameEntries.Add(name);
                m_BindableSaveData.Add(saveInfo);
            }

            m_Persistor.PersistWrites();
        }

        public void RemoveSave(string name)
        {
            m_SaveData.Remove(name);

            int listEntryIndex = m_BindableNameEntries.IndexOf(name);
            if (listEntryIndex != -1)
            {
                m_BindableNameEntries.RemoveAt(listEntryIndex);
                m_BindableSaveData.RemoveAt(listEntryIndex);
            }

            m_Persistor.PersistWrites();
        }

        public bool ContainsSave(string name)
        {
            return m_SaveData.ContainsKey(name);
        }
    }

    [Serializable]
    internal class PlayModeSaveDataInfo : INotifyBindablePropertyChanged
    {
        [SerializeField]
        [DontCreateProperty]
        private string m_Name;

        [CreateProperty]
        public string Name { get => m_Name; set { SetProperty(ref m_Name, value); } }

        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string property = "")
        {
            if (value == null && field == null || value != null && value.Equals(field))
                return;

            field = value;
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
