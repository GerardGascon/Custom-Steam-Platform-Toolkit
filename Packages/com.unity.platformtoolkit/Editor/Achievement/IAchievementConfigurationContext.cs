using System;
using System.Collections.Generic;

namespace Unity.PlatformToolkit.Editor
{
    /// <summary>Context containing currently defined achievements.</summary>
    internal interface IAchievementConfigurationContext
    {
        /// <summary>Invoked after an item is removed within the <see cref="Achievements"/> list.</summary>
        event Action<IAchievement> AchievementRemoved;

        /// <summary>Invoked after an item is added to the <see cref="Achievements"/> list.</summary>
        event Action<IAchievement> AchievementAdded;

        /// <summary>Achievements defined in the Achievements Editor window.</summary>
        IReadOnlyList<IAchievement> Achievements { get; }

    }
}
