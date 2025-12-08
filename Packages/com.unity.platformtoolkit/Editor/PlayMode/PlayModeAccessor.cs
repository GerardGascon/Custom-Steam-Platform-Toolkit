using System;
using UnityEditor;

namespace Unity.PlatformToolkit.PlayMode
{
    internal class PlayModeAccessor : IPlayModeAccessor, IDisposable
    {
        // Cache EditorApplication.isPlaying in a boolean and subscribe to events to monitor changes, since
        // EditorApplication.isPlaying can only be called from the main thread.
        private bool m_IsPlaying;
        public bool IsPlaying => m_IsPlaying;
        public event Action<PlayModeStateChange> OnPlayModeStateChanged;

        public PlayModeAccessor()
        {
            m_IsPlaying = EditorApplication.isPlaying;
            EditorApplication.playModeStateChanged += StateChanged;
        }

        private void StateChanged(PlayModeStateChange newState)
        {
            m_IsPlaying = (newState == PlayModeStateChange.EnteredPlayMode);
            OnPlayModeStateChanged?.Invoke(newState);
        }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= StateChanged;
        }
    }
}
