using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeControlsAssetTracker : AssetPostprocessor
    {
        private static bool s_PlayModeCapabilityAssetsChanged;
        public static event Action PlayModeCapabilityAssetsCreatedOrDeleted;

        public static IEnumerable<PlayModeCapabilityAssetDefinition> GetPlayModeCapabilityAssets()
        {
            return AssetDatabase.FindAssets($"t:{nameof(PlayModeCapabilityAssetDefinition)}").
                Select(guid => AssetDatabase.LoadAssetAtPath<PlayModeCapabilityAssetDefinition>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!s_PlayModeCapabilityAssetsChanged)
            {
                foreach (string path in importedAssets)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(PlayModeCapabilityAssetDefinition))
                    {
                        s_PlayModeCapabilityAssetsChanged = true;
                        break;
                    }
                }
            }

            if(s_PlayModeCapabilityAssetsChanged)
            {
                PlayModeCapabilityAssetsCreatedOrDeleted?.Invoke();
                s_PlayModeCapabilityAssetsChanged = false;
            }
        }

        public static void SetPlayModeCapabilityAssetsChanged()
        {
            s_PlayModeCapabilityAssetsChanged = true;
        }

        internal static IEnumerable<Texture2D> GetInitialAccountPictures()
        {
            var accountPictureFolder = Path.Combine("Packages", "com.unity.platformtoolkit", "EditorResources", "PlayMode", "Accounts");
            string[] assets = AssetDatabase.FindAssets("a:packages t:Texture2D", new string[] { accountPictureFolder });
            return assets.Select(guid => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid)));
        }
    }

    internal class AssetDeletionTracker : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(PlayModeCapabilityAssetDefinition))
            {
                PlayModeControlsAssetTracker.SetPlayModeCapabilityAssetsChanged();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
