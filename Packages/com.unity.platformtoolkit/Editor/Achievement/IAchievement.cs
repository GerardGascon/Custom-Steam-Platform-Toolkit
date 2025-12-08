using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>
    /// Interface that defines a common achievement
    /// </summary>
    internal interface IAchievement : INotifyBindablePropertyChanged
    {
        /// <summary>
        /// The ID for this achievement
        /// </summary>
        [CreateProperty] public string Id { get; }

        /// <summary>
        /// The unlock type of this achievement
        /// </summary>
        [CreateProperty] public UnlockType UnlockType { get; }

        /// <summary>
        /// The progress for this achievement, used depending on <see cref="UnlockType"/>
        /// </summary>
        [CreateProperty] public int ProgressTarget { get; }

        /// <summary>
        /// Contains implementation data.
        /// </summary>
        [CreateProperty]
        public ImplementationData ImplementationData { get; }

    }
}
