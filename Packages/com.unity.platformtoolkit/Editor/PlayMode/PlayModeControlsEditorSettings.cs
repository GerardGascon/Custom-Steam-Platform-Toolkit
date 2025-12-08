using System;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    [FilePath("ProjectSettings/PlayModeControlsEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class PlayModeControlsEditorSettings : ScriptableSingleton<PlayModeControlsEditorSettings>
    {
        internal class DeletionWatcher : AssetModificationProcessor
        {
            private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(PlayModeControlsSettings))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<PlayModeControlsSettings>(path);
                    PlayModeControlsEditorSettings.instance.OnPlayModeSettingsDeleted(asset);
                }

                return AssetDeleteResult.DidNotDelete;
            }
        }

        public event Action OnSettingsAssetChange;

        private void OnPlayModeSettingsDeleted(PlayModeControlsSettings settingsFile)
        {
            settingsFile.Dispose();

            if (settingsFile == m_CurrentSettings)
            {
                CurrentSettings = null;
            }
        }

        [SerializeField]
        private PlayModeControlsSettings m_CurrentSettings;
        public PlayModeControlsSettings CurrentSettings
        {
            get => m_CurrentSettings;
            set
            {
                m_CurrentSettings = value;
                OnSettingsAssetChange?.Invoke();
                Save();
            }
        }

        private void Save()
        {
            Save(true);
        }
    }
}
