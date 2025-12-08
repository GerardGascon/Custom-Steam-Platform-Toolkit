using System;
using UnityEditor;

namespace Unity.PlatformToolkit.PlayMode
{
    /// <summary>
    /// Wrapping Application.isPlaying so that we can mock it in tests
    /// </summary>
    internal interface IPlayModeAccessor
    {
        // <summray>
        // Indicated whether or not the editor is in playmode.
        // </returns>
        public bool IsPlaying { get; }
        public event Action<PlayModeStateChange> OnPlayModeStateChanged;
    }
}
