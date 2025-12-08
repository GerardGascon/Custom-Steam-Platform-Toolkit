using System.Collections;
using System.Collections.Generic;
using Unity.PlatformToolkit.PlayMode;
using UnityEditor;
using UnityEngine;

namespace Unity.PlatformToolkit.Editor
{
    static internal class PlatformToolkitEditorRunner
    {
        static PlayModeControlsSettings s_CachedSettings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            s_CachedSettings = null;

            if (PlayModeControlsEditorSettings.instance.CurrentSettings is { } settings)
            {
                // Always reset the play mode runtime on play to avoid state leaking.
                var newRuntime = settings.RecreateRuntime();
                s_CachedSettings = settings;
                EditorApplication.playModeStateChanged += PlayModeStateChanged;

                var toolkit = new PlayModePlatformToolkit(
                    newRuntime.Capability,
                    newRuntime.Environment,
                    newRuntime.UserManager,
                    settings.LocalSaveData);
#if INPUT_SYSTEM_AVAILABLE
                toolkit.SetInputSystem(newRuntime.PlayModeInputSystem);
#endif // INPUT_SYSTEM_AVAILABLE;
                PlatformToolkit.InjectImplementation(toolkit);
            }
        }

        private static void PlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                // Recreate the runtime for edit-mode
                EditorApplication.playModeStateChanged -= PlayModeStateChanged;

                // Check if the settings asset is still valid (it may have been deleted)
                if (s_CachedSettings != null)
                    s_CachedSettings.RecreateRuntime();
                s_CachedSettings = null;
            }
        }
    }
}
