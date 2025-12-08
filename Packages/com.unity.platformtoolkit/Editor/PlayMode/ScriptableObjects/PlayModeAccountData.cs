using System;
using Unity.Properties;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Stored runtime state (e.g. achievement progress, save games) for a play mode user account.
    /// </summary>
    [Serializable]
    internal class PlayModeAccountData : IDisposable
    {
        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the ScriptableObject that owns this object.
        private ScriptableObjectDataChangePersistor m_Persistor;

        [SerializeField]
        private string m_PublicName = "New User";
        [SerializeField]
        private string m_PrivateName = "New User";
        [SerializeField]
        private Texture2D m_Picture = null;

        [CreateProperty]
        public string PublicName
        {
            get => m_PublicName;
            set
            {
                m_PublicName = value;
                m_Persistor.PersistWrites();
            }
        }

        public string PrivateName
        {
            get => m_PrivateName;
            set
            {
                m_PrivateName = value;
                m_Persistor.PersistWrites();
            }
        }

        [CreateProperty]
        public Texture2D Picture
        {
            get => m_Picture;
            set
            {
                m_Picture = value;
                m_Persistor.PersistWrites();
            }
        }

        [CreateProperty]
        public PlayModeAccountAchievementData Achievements = new();

        [CreateProperty]
        public PlayModeSaveData Saves = new();

        // Used to persist writes in order to make changes visible to the asset inspector.
        // Set by the PlayModeAccountData that owns this object.
        internal ScriptableObjectDataChangePersistor Persistor
        {
            set
            {
                m_Persistor = value;
                Achievements.Persistor = value;
                Saves.Initialize(value);
            }
        }

        internal void Initialize(ScriptableObjectDataChangePersistor persistor)
        {
            Persistor = persistor;
            Achievements.Initialize();
        }

        public void Dispose()
        {
            Achievements.Dispose();
        }
    }
}
